using Backend.Model;

namespace Backend.Database
{
    /// <summary>
    /// This class follows the Singleton pattern. 
    /// It is a collection of List&lt;ISQLModel&gt; that can be used across the application.
    /// </summary>
    public sealed class DatabaseManager
    {
        /// <summary>
        /// The entity map representing the tree structure of the database models.
        /// </summary>
        public static readonly EntityMap Map = new();

        private static readonly Lazy<DatabaseManager> lazyInstance = new(() => new DatabaseManager());
        private readonly List<IAbstractDatabase> Databases;

        /// <summary>
        /// Gets the list of all <see cref="IAbstractDatabase"/> instances.
        /// </summary>
        public static List<IAbstractDatabase> All => lazyInstance.Value.Databases;

        /// <summary>
        /// Adds a database object.
        /// </summary>
        /// <param name="db">An object implementing <see cref="IAbstractDatabase"/>.</param>
        public static void Add(IAbstractDatabase db) => lazyInstance.Value.Databases.Add(db);

        /// <summary>
        /// Removes a database object and disposes of it.
        /// </summary>
        /// <param name="db">An object implementing <see cref="IAbstractDatabase"/>.</param>
        public static void Remove(IAbstractDatabase db)
        {
            lazyInstance.Value.Databases.Remove(db);
            db.Dispose();
        }

        /// <summary>
        /// Gets the number of <see cref="IAbstractDatabase"/> instances.
        /// </summary>
        public static int Count => lazyInstance.Value.Databases.Count;

        private DatabaseManager() => Databases = new List<IAbstractDatabase>();

        /// <summary>
        /// For each database, it concurrently calls the <see cref="IAbstractDatabase.RetrieveAsync(string?, List{QueryParameter}?)"/>.
        /// <para/>
        /// Then, it awaits all tasks to complete and sets for each Database their <see cref="IAbstractDatabase.MasterSource"/> property.
        /// <para/>
        /// For Example:
        /// <code>
        /// await MainDB.Do.FetchData();
        /// RecordSource source = DatabaseManager.Do[0].Records; //get the RecordSource of the first Database;
        /// </code>
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task FetchData()
        {
            List<Task<List<ISQLModel>>> tasks = new List<Task<List<ISQLModel>>>();

            foreach (IAbstractDatabase db in lazyInstance.Value.Databases)
            {
                Map.AddChild(new EntityTree(db.ModelType));
                Task<List<ISQLModel>> task = db.RetrieveAsync().ToListAsync().AsTask();
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            for (int i = 0; i < tasks.Count; i++)
            {
                Get(i).ReplaceRecords(tasks[i].Result);
            }
        }

        /// <summary>
        /// Gets a database based on its zero-based position index.
        /// <para/>
        /// For Example:
        /// <code>
        /// IAbstractDatabase db = DatabaseManager.Do[0]; //get the first IAbstractDatabase;
        /// </code>
        /// </summary>
        /// <param name="index">Zero-based position index.</param>
        /// <returns>An <see cref="IAbstractDatabase"/> instance.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the index is out of range.</exception>
        public static IAbstractDatabase Get(int index) =>
            (index < 0 || index >= lazyInstance.Value.Databases.Count)
            ? throw new IndexOutOfRangeException()
            : lazyInstance.Value.Databases[index];

        /// <summary>
        /// Attempts to find an <see cref="IAbstractDatabase"/> object by 
        /// comparing its <see cref="IAbstractDatabase.Model"/>'s Type Name with the specified name.
        /// </summary>
        /// <param name="name">The name of the <see cref="IAbstractDatabase.Model"/>'s Type Name to find.</param>
        /// <returns>An instance of <see cref="IAbstractDatabase"/> object, or null if not found.</returns>
        public static IAbstractDatabase? Find(string name)
        {
            foreach (IAbstractDatabase db in lazyInstance.Value.Databases)
            {
                if (db.Model.GetType().Name.Equals(name))
                    return db;
            }
            return null;
        }

        /// <summary>
        /// Attempts to find an <see cref="IAbstractDatabase"/> object by 
        /// comparing its <see cref="IAbstractDatabase.Model"/>'s Type with the specified generic type.
        /// </summary>
        /// <typeparam name="M">A type which implements <see cref="ISQLModel"/>.</typeparam>
        /// <returns>An instance of <see cref="IAbstractDatabase"/> object, or null if not found.</returns>
        public static IAbstractDatabase? Find<M>() where M : ISQLModel, new()
        {
            foreach (IAbstractDatabase db in lazyInstance.Value.Databases)
            {
                if (db.Model.GetType().Equals(typeof(M)))
                    return db;
            }
            return null;
        }

        /// <summary>
        /// Maps the database models by adding them to the entity map.
        /// </summary>
        public static void MapUp()
        {
            foreach (IAbstractDatabase db in lazyInstance.Value.Databases)
            {
                Map.AddChild(new EntityTree(db.ModelType));
            }
        }

        /// <summary>
        /// Disposes all database objects and the entity map.
        /// </summary>
        public static void Dispose()
        {
            foreach (IAbstractDatabase db in lazyInstance.Value.Databases)
                db.Dispose();

            Map.Dispose();
        }
    }

}