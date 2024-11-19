using System.Runtime.Serialization;

namespace Arc4u.Security.Principal
{
    /// <summary>
    /// An operation is the atomic entity defining what a user can do.
    /// </summary>
    [DataContract(Namespace = "urn:arc4u.profile.operation")]
    public class Operation
    {

        /// <summary>
        /// Gets or sets the name of the operation.
        /// </summary>
        /// <value>The name.</value>
        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>The ID.</value>
        [DataMember(EmitDefaultValue = false)]
        public int ID { get; set; }

        public override string ToString()
        {
            return String.Format("{0}, {1}.", ID, Name);
        }
    }

    /// <summary>
    /// The scopedRoles are the roles defined for a specific scope.
    /// </summary>
    [DataContract(Namespace = "urn:arc4u.profile.scopedroles")]
    public class ScopedRoles
    {

        /// <summary>
        /// A scope is a subset of the authority defined only for the specific scope
        /// </summary>
        /// <value>The scope.</value>
        [DataMember(EmitDefaultValue = false)]
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets the roles defined in the scope.
        /// </summary>
        /// <value>The roles.</value>
        [DataMember(EmitDefaultValue = false)]
        public List<String> Roles { get; set; }
    }
    /// <summary>
    ///  The ScopedOperations are the operations defined for a specific scope.
    /// </summary>
    [DataContract(Namespace = "urn:arc4u.profile.scopedoperations")]
    public class ScopedOperations
    {

        /// <summary>
        /// A scope is a subset of the authority defined only for the specific scope
        /// </summary>
        /// <value>The scope.</value>
        [DataMember(EmitDefaultValue = false)]
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets the operations defined for the scope.
        /// </summary>
        /// <value>The operations.</value>
        [DataMember(EmitDefaultValue = false)]
        public List<Int32> Operations { get; set; }

    }

    /// <summary>
    /// The Authorization class is the class designed to contains the authority information filled by a AuthorizationFiller and is serializable. 
    /// </summary>
    [DataContract(Namespace = "urn:arc4u.profile.authorization")]
    public class Authorization
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Authorization"/> class.
        /// </summary>
        public Authorization()
        {
            Roles = new List<ScopedRoles>(0);
            Operations = new List<ScopedOperations>(0);
            Scopes = new List<string>(0);

        }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        /// <value>The roles.</value>
        [DataMember(EmitDefaultValue = false)]
        public List<ScopedRoles> Roles { get; set; }

        /// <summary>
        /// Gets or sets the operations.
        /// </summary>
        /// <value>The operations.</value>
        [DataMember(EmitDefaultValue = false)]
        public List<ScopedOperations> Operations { get; set; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>The scopes.</value>
        [DataMember(EmitDefaultValue = false)]
        public List<String> Scopes { get; set; }

        /// <summary>
        /// Return the list of operations so it is possible to show the complete operations list!
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<Operation> AllOperations { get; set; }
    }

}
