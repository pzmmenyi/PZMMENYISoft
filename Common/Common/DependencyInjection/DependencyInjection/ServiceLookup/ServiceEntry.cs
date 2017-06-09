﻿namespace PZMMENYI.DependencyInjection.ServiceLookup {
    internal class ServiceEntry {
        private object _sync = new object();

        public ServiceEntry(IService service) {
            First = service;
            Last = service;
        }

        public IService First { get; private set; }
        public IService Last { get; private set; }

        public void Add(IService service) {
            lock (_sync) {
                Last.Next = service;
                Last = service;
            }
        }
    }
}
