﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18034
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CentralServiceCore.WebTrackerTrackingService {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TrackingType", Namespace="http://schemas.datacontract.org/2004/07/TrackerServices")]
    [System.SerializableAttribute()]
    public partial class TrackingType : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string DescriptionField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int IdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string TitleField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string UrlField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Description {
            get {
                return this.DescriptionField;
            }
            set {
                if ((object.ReferenceEquals(this.DescriptionField, value) != true)) {
                    this.DescriptionField = value;
                    this.RaisePropertyChanged("Description");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Id {
            get {
                return this.IdField;
            }
            set {
                if ((this.IdField.Equals(value) != true)) {
                    this.IdField = value;
                    this.RaisePropertyChanged("Id");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Title {
            get {
                return this.TitleField;
            }
            set {
                if ((object.ReferenceEquals(this.TitleField, value) != true)) {
                    this.TitleField = value;
                    this.RaisePropertyChanged("Title");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Url {
            get {
                return this.UrlField;
            }
            set {
                if ((object.ReferenceEquals(this.UrlField, value) != true)) {
                    this.UrlField = value;
                    this.RaisePropertyChanged("Url");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ClientType", Namespace="http://schemas.datacontract.org/2004/07/TrackerServices")]
    [System.SerializableAttribute()]
    public partial class ClientType : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int ClientIdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string CodeField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int ClientId {
            get {
                return this.ClientIdField;
            }
            set {
                if ((this.ClientIdField.Equals(value) != true)) {
                    this.ClientIdField = value;
                    this.RaisePropertyChanged("ClientId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Code {
            get {
                return this.CodeField;
            }
            set {
                if ((object.ReferenceEquals(this.CodeField, value) != true)) {
                    this.CodeField = value;
                    this.RaisePropertyChanged("Code");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TrackingResponseType", Namespace="http://schemas.datacontract.org/2004/07/TrackerServices")]
    [System.SerializableAttribute()]
    public partial class TrackingResponseType : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool SuccessField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool Success {
            get {
                return this.SuccessField;
            }
            set {
                if ((this.SuccessField.Equals(value) != true)) {
                    this.SuccessField = value;
                    this.RaisePropertyChanged("Success");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="WebsiteUpdateType", Namespace="http://schemas.datacontract.org/2004/07/TrackerServices")]
    [System.SerializableAttribute()]
    public partial class WebsiteUpdateType : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ChangedContentField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.DateTime DateChangedField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string UrlField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ChangedContent {
            get {
                return this.ChangedContentField;
            }
            set {
                if ((object.ReferenceEquals(this.ChangedContentField, value) != true)) {
                    this.ChangedContentField = value;
                    this.RaisePropertyChanged("ChangedContent");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.DateTime DateChanged {
            get {
                return this.DateChangedField;
            }
            set {
                if ((this.DateChangedField.Equals(value) != true)) {
                    this.DateChangedField = value;
                    this.RaisePropertyChanged("DateChanged");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Url {
            get {
                return this.UrlField;
            }
            set {
                if ((object.ReferenceEquals(this.UrlField, value) != true)) {
                    this.UrlField = value;
                    this.RaisePropertyChanged("Url");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="WebTrackerTrackingService.ITrackingService")]
    public interface ITrackingService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITrackingService/TrackNewWebsite", ReplyAction="http://tempuri.org/ITrackingService/TrackNewWebsiteResponse")]
        CentralServiceCore.WebTrackerTrackingService.TrackingResponseType TrackNewWebsite(CentralServiceCore.WebTrackerTrackingService.TrackingType tracking, CentralServiceCore.WebTrackerTrackingService.ClientType client);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITrackingService/TrackNewWebsite", ReplyAction="http://tempuri.org/ITrackingService/TrackNewWebsiteResponse")]
        System.Threading.Tasks.Task<CentralServiceCore.WebTrackerTrackingService.TrackingResponseType> TrackNewWebsiteAsync(CentralServiceCore.WebTrackerTrackingService.TrackingType tracking, CentralServiceCore.WebTrackerTrackingService.ClientType client);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITrackingService/ListTrackings", ReplyAction="http://tempuri.org/ITrackingService/ListTrackingsResponse")]
        CentralServiceCore.WebTrackerTrackingService.TrackingType[] ListTrackings(int lastId, CentralServiceCore.WebTrackerTrackingService.ClientType client);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITrackingService/ListTrackings", ReplyAction="http://tempuri.org/ITrackingService/ListTrackingsResponse")]
        System.Threading.Tasks.Task<CentralServiceCore.WebTrackerTrackingService.TrackingType[]> ListTrackingsAsync(int lastId, CentralServiceCore.WebTrackerTrackingService.ClientType client);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITrackingService/ListAllUpdates", ReplyAction="http://tempuri.org/ITrackingService/ListAllUpdatesResponse")]
        CentralServiceCore.WebTrackerTrackingService.WebsiteUpdateType[] ListAllUpdates(CentralServiceCore.WebTrackerTrackingService.ClientType client);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITrackingService/ListAllUpdates", ReplyAction="http://tempuri.org/ITrackingService/ListAllUpdatesResponse")]
        System.Threading.Tasks.Task<CentralServiceCore.WebTrackerTrackingService.WebsiteUpdateType[]> ListAllUpdatesAsync(CentralServiceCore.WebTrackerTrackingService.ClientType client);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITrackingService/ListWebsiteUpdates", ReplyAction="http://tempuri.org/ITrackingService/ListWebsiteUpdatesResponse")]
        CentralServiceCore.WebTrackerTrackingService.WebsiteUpdateType[] ListWebsiteUpdates(string url, CentralServiceCore.WebTrackerTrackingService.ClientType client);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITrackingService/ListWebsiteUpdates", ReplyAction="http://tempuri.org/ITrackingService/ListWebsiteUpdatesResponse")]
        System.Threading.Tasks.Task<CentralServiceCore.WebTrackerTrackingService.WebsiteUpdateType[]> ListWebsiteUpdatesAsync(string url, CentralServiceCore.WebTrackerTrackingService.ClientType client);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITrackingService/TrackWebsites", ReplyAction="http://tempuri.org/ITrackingService/TrackWebsitesResponse")]
        void TrackWebsites(string[] urls, CentralServiceCore.WebTrackerTrackingService.ClientType client);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITrackingService/TrackWebsites", ReplyAction="http://tempuri.org/ITrackingService/TrackWebsitesResponse")]
        System.Threading.Tasks.Task TrackWebsitesAsync(string[] urls, CentralServiceCore.WebTrackerTrackingService.ClientType client);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITrackingService/UntrackWebsites", ReplyAction="http://tempuri.org/ITrackingService/UntrackWebsitesResponse")]
        void UntrackWebsites(string[] urls, CentralServiceCore.WebTrackerTrackingService.ClientType client);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITrackingService/UntrackWebsites", ReplyAction="http://tempuri.org/ITrackingService/UntrackWebsitesResponse")]
        System.Threading.Tasks.Task UntrackWebsitesAsync(string[] urls, CentralServiceCore.WebTrackerTrackingService.ClientType client);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITrackingService/ListChangedWebsites", ReplyAction="http://tempuri.org/ITrackingService/ListChangedWebsitesResponse")]
        string[] ListChangedWebsites(System.DateTime since, CentralServiceCore.WebTrackerTrackingService.ClientType client);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITrackingService/ListChangedWebsites", ReplyAction="http://tempuri.org/ITrackingService/ListChangedWebsitesResponse")]
        System.Threading.Tasks.Task<string[]> ListChangedWebsitesAsync(System.DateTime since, CentralServiceCore.WebTrackerTrackingService.ClientType client);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ITrackingServiceChannel : CentralServiceCore.WebTrackerTrackingService.ITrackingService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class TrackingServiceClient : System.ServiceModel.ClientBase<CentralServiceCore.WebTrackerTrackingService.ITrackingService>, CentralServiceCore.WebTrackerTrackingService.ITrackingService {
        
        public TrackingServiceClient() {
        }
        
        public TrackingServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public TrackingServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public TrackingServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public TrackingServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public CentralServiceCore.WebTrackerTrackingService.TrackingResponseType TrackNewWebsite(CentralServiceCore.WebTrackerTrackingService.TrackingType tracking, CentralServiceCore.WebTrackerTrackingService.ClientType client) {
            return base.Channel.TrackNewWebsite(tracking, client);
        }
        
        public System.Threading.Tasks.Task<CentralServiceCore.WebTrackerTrackingService.TrackingResponseType> TrackNewWebsiteAsync(CentralServiceCore.WebTrackerTrackingService.TrackingType tracking, CentralServiceCore.WebTrackerTrackingService.ClientType client) {
            return base.Channel.TrackNewWebsiteAsync(tracking, client);
        }
        
        public CentralServiceCore.WebTrackerTrackingService.TrackingType[] ListTrackings(int lastId, CentralServiceCore.WebTrackerTrackingService.ClientType client) {
            return base.Channel.ListTrackings(lastId, client);
        }
        
        public System.Threading.Tasks.Task<CentralServiceCore.WebTrackerTrackingService.TrackingType[]> ListTrackingsAsync(int lastId, CentralServiceCore.WebTrackerTrackingService.ClientType client) {
            return base.Channel.ListTrackingsAsync(lastId, client);
        }
        
        public CentralServiceCore.WebTrackerTrackingService.WebsiteUpdateType[] ListAllUpdates(CentralServiceCore.WebTrackerTrackingService.ClientType client) {
            return base.Channel.ListAllUpdates(client);
        }
        
        public System.Threading.Tasks.Task<CentralServiceCore.WebTrackerTrackingService.WebsiteUpdateType[]> ListAllUpdatesAsync(CentralServiceCore.WebTrackerTrackingService.ClientType client) {
            return base.Channel.ListAllUpdatesAsync(client);
        }
        
        public CentralServiceCore.WebTrackerTrackingService.WebsiteUpdateType[] ListWebsiteUpdates(string url, CentralServiceCore.WebTrackerTrackingService.ClientType client) {
            return base.Channel.ListWebsiteUpdates(url, client);
        }
        
        public System.Threading.Tasks.Task<CentralServiceCore.WebTrackerTrackingService.WebsiteUpdateType[]> ListWebsiteUpdatesAsync(string url, CentralServiceCore.WebTrackerTrackingService.ClientType client) {
            return base.Channel.ListWebsiteUpdatesAsync(url, client);
        }
        
        public void TrackWebsites(string[] urls, CentralServiceCore.WebTrackerTrackingService.ClientType client) {
            base.Channel.TrackWebsites(urls, client);
        }
        
        public System.Threading.Tasks.Task TrackWebsitesAsync(string[] urls, CentralServiceCore.WebTrackerTrackingService.ClientType client) {
            return base.Channel.TrackWebsitesAsync(urls, client);
        }
        
        public void UntrackWebsites(string[] urls, CentralServiceCore.WebTrackerTrackingService.ClientType client) {
            base.Channel.UntrackWebsites(urls, client);
        }
        
        public System.Threading.Tasks.Task UntrackWebsitesAsync(string[] urls, CentralServiceCore.WebTrackerTrackingService.ClientType client) {
            return base.Channel.UntrackWebsitesAsync(urls, client);
        }
        
        public string[] ListChangedWebsites(System.DateTime since, CentralServiceCore.WebTrackerTrackingService.ClientType client) {
            return base.Channel.ListChangedWebsites(since, client);
        }
        
        public System.Threading.Tasks.Task<string[]> ListChangedWebsitesAsync(System.DateTime since, CentralServiceCore.WebTrackerTrackingService.ClientType client) {
            return base.Channel.ListChangedWebsitesAsync(since, client);
        }
    }
}