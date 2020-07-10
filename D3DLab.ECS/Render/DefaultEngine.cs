﻿using D3DLab.ECS.Components;
using D3DLab.ECS.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace D3DLab.ECS.Render {
    public abstract class DefaultEngine {
        static readonly double total = TimeSpan.FromSeconds(1).TotalMilliseconds;
        static readonly double oneFrameMilliseconds = (total / 60);
        //static double _desiredFrameLengthSeconds = 1.0 / 60.0;

        IEntityRenderNotify notify;

        Task loopTask;
        readonly CancellationTokenSource tokensource;
        readonly CancellationToken token;

        public EngineNotificator Notificator { get; }
        public IContextState Context { get; }
        public IRenderableSurface Surface { get; }

        public ElementTag WorldTag { get; }
        public ElementTag CameraTag { get; }


        readonly IInputManager InputManager;
     
        int managedThreadId;

        public DefaultEngine(IRenderableSurface surface, IInputManager inputManager, IContextState context, EngineNotificator notificator) {
            Context = context;
            this.Surface = surface;
            this.Notificator = notificator;
            InputManager = inputManager;
            tokensource = new CancellationTokenSource();
            token = tokensource.Token;

            WorldTag = new ElementTag("World");
            context.GetEntityManager()
                .CreateEntity(WorldTag)
                .AddComponent(new PerfomanceComponent());

            CameraTag = new ElementTag("Camera");
            context.GetEntityManager()
               .CreateEntity(CameraTag);
            context.GetSynchronizationContext().FrameSynchronize(-1);
        }

        protected abstract void Initializing();
        protected abstract ISceneSnapshot CreateSceneSnapshot(InputSnapshot isnap, TimeSpan frameRateTime);

        public void Run(IEntityRenderNotify notify) {
            this.notify = notify;
            Initializing();
            loopTask = Task.Factory.StartNew((Action)Loop, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        protected virtual bool Synchronize() => false;

        void Loop() {
            Thread.CurrentThread.Name = "Game Loop";
            managedThreadId = Thread.CurrentThread.ManagedThreadId;
            var imanager = InputManager;

            //first synchronization
            Context.GetSynchronizationContext().Synchronize(managedThreadId);
            imanager.Synchronize(managedThreadId);

            var speed = new Stopwatch();

            double millisec = oneFrameMilliseconds;
            while (!token.IsCancellationRequested) {
                speed.Restart();

                imanager.Synchronize(managedThreadId);
                var changed = Synchronize();

                var emanager = Context.GetEntityManager();

                Rendering(emanager, imanager, millisec, changed);

                millisec = speed.ElapsedMilliseconds;

                if (millisec < oneFrameMilliseconds) {
                    Thread.Sleep((int)(oneFrameMilliseconds - millisec));
                }
                speed.Stop();

                millisec = speed.ElapsedMilliseconds;

#if DEBUG
                emanager.GetEntity(WorldTag)
                    .UpdateComponent(PerfomanceComponent.Create(millisec, (int)(total / millisec)));
#endif      

                notify.NotifyRender(emanager.GetEntities().ToArray());
            }

            //Window.InputManager.Dispose();
            //Context.Dispose();
        }

        bool Rendering(IEntityManager emanager, IInputManager imanager, double millisec, bool changed) {
            var syncContext = Context.GetSynchronizationContext();

            changed = changed || syncContext.HasChanges;
            syncContext.Synchronize(managedThreadId);

            var isnap = InputManager.GetInputSnapshot();

            if (!isnap.Events.Any() && !changed) {//no input no rendering 
                return false;
            }
            //Context.GetGeometryPool().Synchronize(managedThreadId);
            //Context.GetOctreeManager().Synchronize(managedThreadId);

            var snapshot = CreateSceneSnapshot(isnap, TimeSpan.FromMilliseconds(millisec));// new SceneSnapshot(Window, notificator, viewport, Octree, ishapshot, TimeSpan.FromMilliseconds(millisec));
            foreach (var sys in Context.GetSystemManager().GetSystems()) {
                try {
                    sys.Execute(snapshot);
                    //run synchronization after each exetuted system, to synchronize state for the next system
                    syncContext.FrameSynchronize(managedThreadId);
                } catch (Exception ex) {
                    ex.ToString();
#if !DEBUG
                    throw ex;
#endif
                }
            }
            return true;
        }

        public virtual void Dispose() {
            if (loopTask.Status == TaskStatus.Running) {
                tokensource.Cancel();
                loopTask.Wait();
            }
            loopTask.Dispose();
        }


    }
}
