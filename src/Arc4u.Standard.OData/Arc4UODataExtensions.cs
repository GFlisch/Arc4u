using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Arc4u.Standard.OData
{
    /// <summary>
    /// These extensions are required to make sure all URI's that our OData replies expose are relative to Yarp's address, not the address of our local service.
    /// </summary>
    public static class Arc4UODataExtensions
    {
        /// <summary>
        /// Make sure that the service root URI used in OData serialization points to the endpoint used by external consumers (as understood by YARP).
        /// OData can embed URI's to its own service in some scenarios, like pagination ("@odata.nextLink").
        /// Call this method when setting up OData when configuring our service collection.
        /// </summary>
        /// <param name="configureServices">The service collection associated with the OData route components</param>
        /// <param name="odataBaseAddress">
        /// The base address of the service as seen by external consumers (i.e. from the point of view of YARP)
        /// Can be null if the service is being tested standalone, not behind YARP.
        /// </param>
        /// <returns>The service collection passed as parameter</returns>
        /// <example>
        ///     services.AddControllers()
        ///         ...
        ///        .AddOData(options => options
        ///            ...
        ///            .AddRouteComponents("odata", ODataModel.GetEdmModel(), configureServices => configureServices.AddODataSerializerBaseAddress(odataBaseAddress))
        ///            )
        ///        ...
        /// </example>
        public static IServiceCollection AddODataSerializerBaseAddress(this IServiceCollection configureServices, Uri odataBaseAddress)
        {
            if (odataBaseAddress != null)
                configureServices.AddSingleton<IODataSerializerProvider>(rootContainer => new Arc4UODataSerializerProvider(rootContainer, odataBaseAddress));
            return configureServices;
        }


        /// <summary>
        /// Make sure OData's metadata URI's ("@odata.context") point to the endpoint used by external consumers (as understood by YARP).
        /// </summary>
        /// <param name="options">The MVC configuration options</param>
        /// <param name="odataBaseAddress">
        /// The base address of the service as seen by external consumers (i.e. from the point of view of YARP)
        /// Can be null if the service is being tested standalone, not behind YARP.
        /// </param>
        /// <returns>The MVC configuration options passed as parameter</returns>
        /// <example>
        ///     services.AddControllers()
        ///         ...
        ///        .AddOData(...)
        ///        ...
        ///        .AddMvcOptions(options => options.SetODataFormattersBaseAddress(odataBaseAddress));
        /// </example>
        public static MvcOptions SetODataFormattersBaseAddress(this MvcOptions options, Uri odataBaseAddress)
        {
            // we need to provide an alternative base address for the OData metadata URI because we are behind a reverse proxy (YARP)
            if (odataBaseAddress != null)
            {
                foreach (var outputFormatter in options.OutputFormatters.OfType<ODataOutputFormatter>())
                    outputFormatter.BaseAddressFactory = request => odataBaseAddress;
                foreach (var inputFormatter in options.InputFormatters.OfType<ODataInputFormatter>())
                    inputFormatter.BaseAddressFactory = request => odataBaseAddress;
            }
            return options;
        }
    }
}
