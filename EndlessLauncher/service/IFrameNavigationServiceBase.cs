using GalaSoft.MvvmLight.Views;

namespace EndlessLauncher.service
{
    public interface IFrameNavigationService : INavigationService
    {
        object Parameter { get; }
    }
}
