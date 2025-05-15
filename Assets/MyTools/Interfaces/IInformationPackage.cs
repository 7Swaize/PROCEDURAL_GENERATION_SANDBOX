namespace VS.Utilities.Interfaces
{
    public interface IInformationPackage<out T> where T : struct
    {
        public T GetDefault();
        public T GetCurrent();
    }
}