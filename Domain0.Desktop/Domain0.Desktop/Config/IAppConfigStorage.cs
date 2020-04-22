namespace Domain0.Desktop.Config
{
    public interface IAppConfigStorage
    {
        void Save(AppConfig value);
        AppConfig Load();
    }
}