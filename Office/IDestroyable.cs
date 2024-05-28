namespace Backend.Office
{
    public interface IDestroyable
    {
        /// <summary>
        /// It perform the memory clean-up by calling Marshal.ReleaseComObject on the COM object.
        /// </summary>
        void Destroy();
    }
}
