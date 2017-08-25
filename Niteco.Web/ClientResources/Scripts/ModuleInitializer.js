define([
// Dojo
        "dojo",
        "dojo/_base/declare",
        "dojo/aspect",
//CMS
        "epi/_Module",
        "epi/dependency",
        "epi/routes",
        "epi/cms/store/CustomQueryEngine"
        //"niteco/press/QCT/MyCommandProvider"
], function (
// Dojo
        dojo,
        declare,
        aspect,
//CMS
        _Module,
        dependency,
        routes,
        CustomQueryEngine
    //MyCommandProvider
    ) {

    return declare("niteco/ModuleInitializer", [_Module], {
        initialize: function () {

            this.inherited(arguments);

            var registry = this.resolveDependency("epi.storeregistry");
            registry.create("technologiesstore", this._getRestPath("technologiesstore"));
        },

        _getRestPath: function (name) {
            return routes.getRestPath({ moduleArea: "app", storeName: name });
        }
    });
});