using Backend.Model;

namespace Backend.Database
{
    /// <summary>
    /// This class follows the Singleton pattern. 
    /// It is a collections of List&lt;ISQLModel> that can be used accross the application.
    /// </summary>
    public sealed class DatabaseManager
    {
        public static readonly EntityMap Map = new();
        private static readonly Lazy<DatabaseManager> lazyInstance = new(() => new DatabaseManager());
        private readonly List<IAbstractDatabase> Databases;

        public static List<IAbstractDatabase> All { get => lazyInstance.Value.Databases; }

        /// <summary>
        /// It adds a database object.
        /// </summary>
        /// <param name="db">An object implementing <see cref="IAbstractDatabase"/></param>
        public static void Add(IAbstractDatabase db) => lazyInstance.Value.Databases.Add(db);

        /// <summary>
        /// The number of IAbstractDatabases.
        /// </summary>
        /// <value>An Integer.</value>
        public static int Count => lazyInstance.Value.Databases.Count;   
        private DatabaseManager() => Databases = [];


        /// <summary>
        /// For each database, it calls concurrently, the <see cref="IAbstractDatabase.RetrieveAsync(string?, List{QueryParameter}?)"/>.
        /// <para/>
        /// Then, it awaits for all tasks to complete and sets for each Database their <see cref="IAbstractDatabase.MasterSource"/> property. 
        /// You can access the Database by calling <see cref="Do"/>
        /// <para/>
        /// For Example:
        /// <code>
        /// await MainDB.Do.FetchData();
        /// RecordSource source = DatabaseManager.Do[0].Records; //get the RecordSource of the first Database;
        /// </code>
        /// </summary>
        /// <returns>A Task</returns>
        public static async Task FetchData() 
        {
            List<Task<List<ISQLModel>>> tasks = [];

            foreach (IAbstractDatabase db in lazyInstance.Value.Databases)
            {
                Map.AddChild(new(db.ModelType));
                Task<List<ISQLModel>> task = db.RetrieveAsync().ToListAsync().AsTask();
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            for(int i = 0; i < tasks.Count; i++) Get(i).ReplaceRecords(tasks[i].Result);

        }

        /// <summary>
        /// Gets a Database based on its zero-based position index.
        /// <para/>
        /// For Example:
        /// <code>
        /// IAbstractDatabase db = DatabaseManager.Do[0]; //get the first IAbstractDatabase;
        /// </code>
        /// </summary>
        /// <param name="index">zero-based position.</param>
        /// <returns>An IAbstractDatabase</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static IAbstractDatabase Get(int index) => (index < 0 || index >= lazyInstance.Value.Databases.Count) ? throw new IndexOutOfRangeException() : lazyInstance.Value.Databases[index];


        /// <summary>
        /// Attempts to find a <see cref="IAbstractDatabase"/> object by 
        /// comparing its <see cref="IAbstractDatabase.Model"/>'s Type Name and the <param name="name">name</param> argument.
        /// </summary>
        /// <param name="name">The name of the <see cref="IAbstractDatabase.Model"/>'s Type Name to find</param>
        /// <returns>An instance of <see cref="IAbstractDatabase"/> object. Returns null if the instance was not found.</returns>
        public static IAbstractDatabase? Find(string name) 
        { 
            foreach(IAbstractDatabase db in lazyInstance.Value.Databases) 
                if (db.Model.GetType().Name.Equals(name)) return db;
            return null;
        }

        /// <summary>
        /// Attempts to find a <see cref="IAbstractDatabase"/> object by 
        /// comparing its <see cref="IAbstractDatabase.Model"/>'s Type and the <typeparam name="M">M</typeparam> generic.
        /// </summary>
        /// <typeparam name="M">A type which implements <see cref="ISQLModel"/></typeparam>
        /// <returns>An instance of <see cref="IAbstractDatabase"/> object. Returns null if the instance was not found.</returns>
        public static IAbstractDatabase? Find<M>() where M : ISQLModel, new()
        {
            foreach (IAbstractDatabase db in lazyInstance.Value.Databases)
            {
                if (db.Model.GetType().Equals(typeof(M))) return db;
            }
            return null;
        }

        public static void MapUp()
        {
            foreach (IAbstractDatabase db in lazyInstance.Value.Databases)
            {
                Map.AddChild(new(db.ModelType));
            }
        }

        public static void Dispose()
        {
            foreach (IAbstractDatabase db in lazyInstance.Value.Databases)
                db.Dispose();
            
            Map.Dispose();
        }
    }

}