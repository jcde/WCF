using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Principal;
using System.ServiceModel;
using ServiceModelEx;
using WcfDomain.Contracts;
using WcfDomain.Contracts.Security;

namespace WcfServer
{
    /// <summary>
    /// in order to switch security off, don't instantiate this class at all
    /// </summary>
    public class ClaimsPolicy : IAuthorizationPolicy
    {
        private static readonly ClaimSet _issuer
            = new DefaultClaimSet(ClaimSet.System, // issuer should NOT be Self
                                  Claim.CreateNameClaim(Namespaces.ClaimsIssuer));

        private static bool _enabled;

        private readonly Guid _id = Guid.NewGuid();

        public ClaimsPolicy()
        {
            _enabled = true;
        }

        #region IAuthorizationPolicy Members

        public ClaimSet Issuer
        {
            get { return _issuer; }
        }

        public string Id
        {
            get { return _id.ToString(); }
        }

        /// <summary>
        /// called for every server operation
        /// </summary>
        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            if (evaluationContext.Properties.ContainsKey("Identities"))
            {
                var identities = (List<IIdentity>) evaluationContext.Properties["Identities"];
                IIdentity identity = identities[0];

                ClaimSet claims = MapClaims(identity);

                var newPrincipal = new GenericPrincipal(identity, null);
                evaluationContext.Properties["Principal"] = newPrincipal;

                if (claims != null)
                    evaluationContext.AddClaimSet(this, claims);

                return true;
            }
            return false;
        }

        #endregion

        protected virtual ClaimSet MapClaims(IIdentity p)
        {
            var listClaims = new List<Claim>();

            if (p is WindowsIdentity
                && new WindowsPrincipal((WindowsIdentity) p).IsInRole(WindowsBuiltInRole.Administrator))
            {
                listClaims.Add(new Claim(ClaimType.All.ToString(),
                                         SecureResource.All, Rights.PossessProperty));
            }
            return new DefaultClaimSet(_issuer, listClaims);
        }

        /// <summary>
        /// must be called from Service Method
        /// </summary>
        public static MethodInfo CurrentMethod()
        {
            if (OperationContext.Current != null)
            {
                string action = OperationContext.Current.IncomingMessageHeaders.Action;
                string[] slashes = action.Split('/');
                string methodName = slashes[slashes.Length - 1];
                string contractName = slashes[slashes.Length - 2];
                Dictionary<string, MethodInfo> meths = MethodsManager.GetMethods(contractName);
                if (meths != null && meths.ContainsKey(methodName))
                    return meths[methodName];
            }
            return null;
        }

        /// <summary>
        /// must be called from Service Method
        /// </summary>
        public static Type CurrentContact()
        {
            if (OperationContext.Current != null)
            {
                string action = OperationContext.Current.IncomingMessageHeaders.Action;
                string[] slashes = action.Split('/');
                string contractName = slashes[slashes.Length - 2];
                return MethodsManager.GetContract(contractName);
            }
            return null;
        }

        public static void ServerCheck()
        {
            if (_enabled && OperationContext.Current != null)
            {
                MethodInfo meth = CurrentMethod();
                if (meth != null)
                    ServerMethodCheck(meth);
            }
        }

        public static void ServerMethodCheck(MethodInfo contractMethod)
        {
            if (_enabled)
                foreach (
                    OperationPermissionAttribute a in
                        contractMethod.GetCustomAttributes(typeof (OperationPermissionAttribute), true))
                {
                    if (a.SecurityAction == SecurityAction.Demand)
                    {
                        ClaimDemand(a.ClaimType, a.SecureResource);
                    }
                }
        }

        private static void ClaimDemand(ClaimType claim, SecureResource resource)
        {
            AuthorizationContext authContext = ServiceSecurityContext.Current.AuthorizationContext;

            ClaimSet issuerClaimSet = null;
            foreach (ClaimSet cs in authContext.ClaimSets)
            {
                if (cs.Issuer == _issuer)
                {
                    issuerClaimSet = cs;
                    break;
                }
            }

            if (issuerClaimSet == null)
                throw new PermissionException(string.Format("No claims for issuer {0} were provided.",
                                                            _issuer[0].Resource));

            var c = new Claim(ClaimType.All.ToString(), SecureResource.All, Rights.PossessProperty);
            if (issuerClaimSet.ContainsClaim(c)) // if administrator
                return;

            if (claim != ClaimType.All)
            {
                c = new Claim(ClaimType.All.ToString(), resource, Rights.PossessProperty);
                if (issuerClaimSet.ContainsClaim(c))
                    return;
            }

            if (resource != SecureResource.All)
            {
                c = new Claim(claim.ToString(), SecureResource.All, Rights.PossessProperty);
                if (issuerClaimSet.ContainsClaim(c))
                    return;
            }

            c = new Claim(claim.ToString(), resource, Rights.PossessProperty);
            if (issuerClaimSet.ContainsClaim(c))
                return;

            throw new PermissionException(string.Format("Claim {0} for resource {1} is not satisfied.",
                                                        claim, resource));
        }
    }
}