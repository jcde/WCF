﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WcfServer.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("WcfServer.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Access denied.
        /// </summary>
        public static string AccessDenied {
            get {
                return ResourceManager.GetString("AccessDenied", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is already created.
        /// </summary>
        public static string ChatRoomExists {
            get {
                return ResourceManager.GetString("ChatRoomExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Chat Room was reused.
        /// </summary>
        public static string ChatRoomReused {
            get {
                return ResourceManager.GetString("ChatRoomReused", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is wrong room name.
        /// </summary>
        public static string ChatRoomWrong {
            get {
                return ResourceManager.GetString("ChatRoomWrong", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} has already joined {1}.
        /// </summary>
        public static string ChatUserAlreadyJoined {
            get {
                return ResourceManager.GetString("ChatUserAlreadyJoined", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to only Moderator can join other some client.
        /// </summary>
        public static string ModeratorJoin {
            get {
                return ResourceManager.GetString("ModeratorJoin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to only Moderator can punt other some client.
        /// </summary>
        public static string ModeratorLeave {
            get {
                return ResourceManager.GetString("ModeratorLeave", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Moderator is needed. Chat Room does not have..
        /// </summary>
        public static string ModeratorNull {
            get {
                return ResourceManager.GetString("ModeratorNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Only joined users can do this action.
        /// </summary>
        public static string NotJoined {
            get {
                return ResourceManager.GetString("NotJoined", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to only explicitly allowed users can join not public room.
        /// </summary>
        public static string PrivateRoomAllowedUsersOnly {
            get {
                return ResourceManager.GetString("PrivateRoomAllowedUsersOnly", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Join to chat was rejected.
        /// </summary>
        public static string RejectedJoin {
            get {
                return ResourceManager.GetString("RejectedJoin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Following settings&apos; modification: &quot;ServerListenPort, ServerMexPort, AllowClients, WcfSecurityEnabled,ClaimsSecurityEnabled&quot; will give effect only after server restart..
        /// </summary>
        public static string RestartedSettings {
            get {
                return ResourceManager.GetString("RestartedSettings", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Server did not start yet.
        /// </summary>
        public static string ServiceNotRun {
            get {
                return ResourceManager.GetString("ServiceNotRun", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User is not available.
        /// </summary>
        public static string UserNotAvailable {
            get {
                return ResourceManager.GetString("UserNotAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User is not connected. Please wait until it will connect..
        /// </summary>
        public static string UserNotConnected {
            get {
                return ResourceManager.GetString("UserNotConnected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Wrong user.
        /// </summary>
        public static string UserWrong {
            get {
                return ResourceManager.GetString("UserWrong", resourceCulture);
            }
        }
    }
}
