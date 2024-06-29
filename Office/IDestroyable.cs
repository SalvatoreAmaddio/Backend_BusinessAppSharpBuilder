namespace Backend.Office
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// Provides a mechanism for releasing unmanaged resources associated with COM objects.
    /// </summary>
    public interface IDestroyable
    {
        /// <summary>
        /// Performs memory clean-up by calling <see cref="Marshal.ReleaseComObject"/> on the COM object.
        /// This method should be called to release unmanaged resources and avoid memory leaks.
        /// </summary>
        void Destroy();
    }

}
