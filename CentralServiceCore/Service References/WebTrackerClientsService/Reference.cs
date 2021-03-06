﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18034
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CentralServiceCore.WebTrackerClientsService {
    using System.Runtime.Serialization;
    using System;
    
    
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
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="WebTrackerClientsService.IClientsService")]
    public interface IClientsService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IClientsService/RegisterClient", ReplyAction="http://tempuri.org/IClientsService/RegisterClientResponse")]
        CentralServiceCore.WebTrackerClientsService.ClientType RegisterClient();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IClientsService/RegisterClient", ReplyAction="http://tempuri.org/IClientsService/RegisterClientResponse")]
        System.Threading.Tasks.Task<CentralServiceCore.WebTrackerClientsService.ClientType> RegisterClientAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IClientsServiceChannel : CentralServiceCore.WebTrackerClientsService.IClientsService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ClientsServiceClient : System.ServiceModel.ClientBase<CentralServiceCore.WebTrackerClientsService.IClientsService>, CentralServiceCore.WebTrackerClientsService.IClientsService {
        
        public ClientsServiceClient() {
        }
        
        public ClientsServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ClientsServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ClientsServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ClientsServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public CentralServiceCore.WebTrackerClientsService.ClientType RegisterClient() {
            return base.Channel.RegisterClient();
        }
        
        public System.Threading.Tasks.Task<CentralServiceCore.WebTrackerClientsService.ClientType> RegisterClientAsync() {
            return base.Channel.RegisterClientAsync();
        }
    }
}
