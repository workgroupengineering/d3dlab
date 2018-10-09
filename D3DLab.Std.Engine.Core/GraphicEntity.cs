﻿using System;
using System.Collections.Generic;

namespace D3DLab.Std.Engine.Core {

    public sealed class GraphicEntity  {
        public ElementTag Tag { get; }
        readonly IComponentManager manager;
        readonly EntityOrderContainer order;

        public GraphicEntity(ElementTag tag, IComponentManager manager, EntityOrderContainer order) {
            this.order = order;
            this.manager = manager;
            Tag =tag;
           
        }

        public T GetComponent<T>() where T : IGraphicComponent {
            return manager.GetComponent<T>(Tag);
        }
        public IEnumerable<T> GetComponents<T>() where T : IGraphicComponent {
            return manager.GetComponents<T>(Tag);
        }

        public GraphicEntity AddComponent<T>(T component) where T : IGraphicComponent {
            manager.AddComponent(Tag, component);
            return this;
        }
        public void RemoveComponent(IGraphicComponent component) {
            manager.RemoveComponent(Tag, component);            
        }
        public void RemoveComponentsOfType<TCom>() where TCom : IGraphicComponent {
            foreach (var component in manager.GetComponents<TCom>(Tag)) {
                manager.RemoveComponent(Tag, component);
            }
        }
        

        public bool Has<T>() where T : IGraphicComponent {
            return manager.Has<T>(Tag);
        }
        public IEnumerable<IGraphicComponent> GetComponents() {
            return manager.GetComponents(Tag);
        }

        public int GetOrderIndex<TSys>()
            where TSys : IComponentSystem  {
            return order.Get<TSys>(Tag);
        }
    }
    public class OrderSystemContainer : Dictionary<Type, int> {

    }
    public class EntityOrderContainer {
        readonly Dictionary<ElementTag, OrderSystemContainer> componentOrderIndex;
        readonly Dictionary<Type, int> systemsOrder;

        public EntityOrderContainer() {
            componentOrderIndex = new Dictionary<ElementTag, OrderSystemContainer>();
            systemsOrder = new Dictionary<Type, int>();
        }
        public EntityOrderContainer RegisterOrder<TSys>(ElementTag tag,int index) 
            where TSys : IComponentSystem{
            OrderSystemContainer ordering;
            if (!componentOrderIndex.TryGetValue(tag, out ordering)) {
                ordering = new OrderSystemContainer();
                componentOrderIndex.Add(tag, ordering);
            }
            var t = typeof(TSys);

            ordering.Add(t, index);
            IncrementSystemOrderIndex(t);

            return this;
        }

        public EntityOrderContainer RegisterOrder<TSys>(ElementTag tag)
           where TSys : IComponentSystem {
            OrderSystemContainer ordering;
            if (!componentOrderIndex.TryGetValue(tag, out ordering)) {
                ordering = new OrderSystemContainer();
                componentOrderIndex.Add(tag, ordering);
            }
            var t = typeof(TSys);

            ordering.Add(t, IncrementSystemOrderIndex(t));

            return this;
        }

        public int Get<TSys>(ElementTag tag)
            where TSys : IComponentSystem {
            if (!componentOrderIndex.ContainsKey(tag)) {
                return int.MaxValue;
            }
            return componentOrderIndex[tag][typeof(TSys)];
        }

        int IncrementSystemOrderIndex(Type t) {
            if (!systemsOrder.ContainsKey(t)) {
                systemsOrder.Add(t, 0);
            } else {
                systemsOrder[t] = systemsOrder[t] + 1;
            }
            return systemsOrder[t];
        }
    }
}