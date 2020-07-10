﻿using D3DLab.Debugger;
using D3DLab.ECS;
using D3DLab.Viewer.Infrastructure;
using D3DLab.Viewer.Presentation;
using D3DLab.Viewer.Presentation.FileDetails;
using D3DLab.Viewer.Presentation.OpenFiles;
using D3DLab.Viewer.Presentation.TopPanel.SaveAll;
using Microsoft.Extensions.DependencyInjection;

using System.Windows;

using WPFLab;
using WPFLab.Messaging;

namespace D3DLab.Viewer {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class LabApp : LabApplication {
        public LabApp() {

        }

        protected override void ConfigureServices(IDependencyRegisterService registrator) {
            registrator
                .RegisterApplication(this)
                .RegisterAsSingleton<IMessenger, Messenger>()
                .RegisterUnhandledExceptionHandler()
                .Register<AppLogger>()
                .Register<IAppLogger>(x => x.GetService<AppLogger>())
                .Register<ILabLogger>(x => x.GetService<AppLogger>())
                //
                .Register<AppSettings>()
                .RegisterView<MainWindow>()
                .Register<MainWindowViewModel>()
                .Register<IFileLoader>(x => x.GetService<MainWindowViewModel>())
                .Register<ISaveLoadedObject>(x => x.GetService<MainWindowViewModel>())
                //               
                .RegisterMvvm()

                //dialogs 
                .RegisterDebugger()
                .RegisterTransient<OpenFilesViewModel>().RegisterTransientView<OpenFilesWindow>()
                .RegisterTransient<ObjDetailsViewModel>().RegisterTransientView<ObjDetailsWindow>()
                .RegisterTransient<SaveAllViewModel>().RegisterTransientView<SaveAllWindow>()
                .Register<DialogManager>()
                ;
        }

        protected override void AppStartup(StartupEventArgs e, IDependencyResolverService resolver) {
            resolver.UseUnhandledExceptionHandler();
            resolver.ResolveView<MainWindow, MainWindowViewModel>().Show();
        }

        protected override void AppExitp(ExitEventArgs e, IDependencyResolverService resolver) {
            resolver.RemoveUnhandledExceptionHandler();
        }
    }
}
