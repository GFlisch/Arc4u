using Arc4u.ServiceModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Arc4u.Data
{
    /// <summary>
    /// Represents a <see cref="NotifyEntity"/> self-tracking its <see cref="PersistChange"/>.
    /// </summary>
    [DataContract]
    public abstract class PersistEntity : NotifyEntity, IPersistEntity
    {
        /// <ignore/>
        protected const string PersistChangePropertyName = "PersistChange";

        /// <ignore/>
        protected const string InvalidTransition = "Invalid transition between {0} and {1} change.";

        #region IPersistChange Members

        /// <ignore/>
        protected PersistChange _persistChange;

        /// <summary>
        /// Gets or sets the persist change of the current <see cref="PersistEntity"/>.
        /// </summary>
        /// <value>The persist change.</value>
        [DataMember(EmitDefaultValue = false)]
        public virtual PersistChange PersistChange
        {
            get { return _persistChange; }
            set
            {
                if (_persistChange != value)
                {
                    // prevent invalid transitions except when serializing / deserializing
                    if (!IgnoreOnPropertyChanged
                        && (_persistChange == PersistChange.Insert && value == PersistChange.Delete
                            || _persistChange == PersistChange.Delete && value == PersistChange.Insert))
                    {
                        throw new ArgumentException(string.Format(InvalidTransition, _persistChange, value));
                    }

                    _persistChange = value;
                    RaisePropertyChanged(PersistChangePropertyName);
                }
            }
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raises the <see cref="NotifyEntity.PropertyChanged"/> event, 
        /// except when the <see cref="NotifyEntity.IgnoreOnPropertyChanged"/> property is set to <c>true</c>,
        /// after touching the entity if the <see cref="PropertyChangedEventArgs.PropertyName"/> is a touching property,
        /// </summary>
        /// <param name="e">A <see cref="PropertyChangedEventArgs"/> that contains the event data.</param>
        protected override void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            if (IgnoreOnPropertyChanged)
                return;

            base.RaisePropertyChanged(e);
        }

        /// <summary>
        /// Determines whether if the specified <see cref="PropertyChangedEventArgs"/> 
        /// contains a property touching the entity.
        /// </summary>
        /// <param name="e">A <see cref="PropertyChangedEventArgs"/> that contains a <see cref="PropertyChangedEventArgs.PropertyName"/>.</param>
        /// <returns>
        ///   <c>true</c> if the contained property name is touching the entity; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is null.</exception>
        protected virtual bool IsTouchingProperty(PropertyChangedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            return !string.Equals(e.PropertyName, PersistChangePropertyName, StringComparison.Ordinal);
        }

        #endregion

        #endregion

        /// <summary>
        /// Converts the value of the current <see cref="PersistEntity"/> object to its equivalent string representation. 
        /// </summary>
        /// <returns>A string representation of the value of the current <see cref="PersistEntity"/> object.</returns>
        public override string ToString()
        {
            return string.Format("PersistChange = {0}, {1}", PersistChange, base.ToString());
        }

        #region Deserialization Members

        /// <ignore/>
        /// <remarks>This methods must be public; otherwise Silverlight serializers will not be able to use it.</remarks>
        [OnDeserializing]
        public void OnDeserializing(StreamingContext context)
        {
            IgnoreOnPropertyChanged = true;
        }

        /// <ignore/>        
        /// <remarks>This methods must be public; otherwise Silverlight serializers will not be able to use it.</remarks>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            IgnoreOnPropertyChanged = false;
        }

        #endregion

        protected PersistEntity() : this(PersistChange.None)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistEntity"/> class 
        /// with the specified <see cref="PersistChange"/>.
        /// </summary>
        /// <param name="persistChange">The persist change.</param>
        protected PersistEntity(PersistChange persistChange)
        {
            _persistChange = persistChange;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistEntity"/> class
        /// copied from the specified <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entity"/> is null.</exception>
        protected PersistEntity(PersistEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _persistChange = entity._persistChange;
        }

        public Messages TryValidate()
        {
            var messages = new Messages();
            var results = new List<ValidationResult>();
            var context = new ValidationContext(this);
            if (!Validator.TryValidateObject(this, context, results, true))
            {
                results.ForEach(
                    r => messages.Add(new Message(ServiceModel.MessageCategory.Business, ServiceModel.MessageType.Error, r.ErrorMessage)));
            }
            return messages;
        }

        public void Validate<T>(ILogger<T> logger)
        {
            TryValidate().LogAndThrowIfNecessary(logger);
        }
    }
}