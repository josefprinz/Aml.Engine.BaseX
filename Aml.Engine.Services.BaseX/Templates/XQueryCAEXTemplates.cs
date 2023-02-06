using Aml.Engine.CAEX;

namespace Aml.Engine.Services.BaseX.Templates
{
    public static class XQueryCAEXTemplates
    {
        #region Methods

        /// <summary>
        /// Gets the query template for the header of this caex object.
        /// Child elements are included if they are not from the same object type.
        /// This is done to avoid reading deep hierarchies.
        /// </summary>
        /// <param name="CAEXObject"></param>
        /// <returns></returns>
        public static string CAEXElementsTemplate(string caexTagname)
        {
            return caexTagname switch
            {
                CAEX_CLASSModel_TagNames.INTERNALELEMENT_STRING =>
                                        " {$objectNode/Attribute}" +
                                        " {$objectNode/ExternalInterface}" +
                                        " {$objectNode/SupportedRoleClass}" +
                                        " {$objectNode/InternalLink}" +
                                        " {$objectNode/RoleRequirements}",

                CAEX_CLASSModel_TagNames.SYSTEMUNITCLASS_STRING =>
                                        " {$objectNode/Attribute}" +
                                        " {$objectNode/ExternalInterface}" +
                                        " {$objectNode/SupportedRoleClass}" +
                                        " {$objectNode/InternalLink}",

                CAEX_CLASSModel_TagNames.INTERFACECLASS_STRING => 
                                        " {$objectNode/Attribute}" +
                                        " {$objectNode/ExternalInterface}",

                CAEX_CLASSModel_TagNames.ROLECLASS_STRING =>
                                        " {$objectNode/Attribute}" +
                                        " {$objectNode/ExternalInterface}",

                CAEX_CLASSModel_TagNames.ATTRIBUTETYPE_STRING => 
                                        " {$objectNode/Attribute}",
                _ => "",
            };
        }

        public static string CAEXFileHeaderTemplate(string database, string document) =>
            $"let $objectNode:=doc('{database}/{document}')/CAEXFile" +
            " return <CAEXFile>" +
            " {$objectNode/@*}" +
            " {$objectNode/Description}" +
            " {$objectNode/Version}" +
            " {$objectNode/Revision}" +
            " {$objectNode/Copyright}" +
            " {$objectNode/SourceObjectInformation}" +
            " {$objectNode/AdditionalInformation}" +
            " {$objectNode/SuperiorStandardVersion}" +
            " {$objectNode/SourceDocumentInformation}" +
            " {$objectNode/ExternalReference}" +
            " </CAEXFile>";

        public static string CAEXHeaderTemplate(string CAEXTagName) =>
            $" return <{CAEXTagName}>" +
            " {$objectNode/@*}" +
            " {$objectNode/Description}" +
            " {$objectNode/Version}" +
            " {$objectNode/Revision}" +
            " {$objectNode/Copyright}" +
            " {$objectNode/SourceObjectInformation}" +
            " {$objectNode/AdditionalInformation} " + 
            CAEXElementsTemplate(CAEXTagName) +
            $" </{CAEXTagName}>";


        public static string IterativeElementsTemplate(string database, string document, string path, string caexTagname) =>
             $"let $root:=doc('{database}/{document}'){path}" +
             " return <XElements> {" +
             $" for $objectNode in $root/{caexTagname}" +
             CAEXHeaderTemplate(caexTagname) +
            "} </XElements>";

        #endregion Methods
    }
}