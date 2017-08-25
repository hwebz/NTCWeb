define([
        "dojo",
        "dojo/_base/array",
        "dojo/_base/declare",
        "dojo/_base/Deferred",
        "dojo/_base/lang",
        "dojo/DeferredList",
        "dojo/dom-attr",
        "dojo/dom-class",
        "dojo/dom-construct",
        "dojo/dom-style",
        "dojo/query",
        "dijit/_CssStateMixin",
        "dijit/layout/_LayoutWidget",
        "dijit/_TemplatedMixin",
        "dijit/_Container",
        "dijit/_Widget",
        "dijit/_WidgetsInTemplateMixin",
        "dijit/Tooltip",
        "dijit/form/ComboBox",
        "dijit/form/FilteringSelect",
        "dojox/html/entities",
        "epi/shell/widget/_ValueRequiredMixin",
        "epi/dependency",
        "epi/epi",
        "epi/shell/_ContextMixin",
        "niteco/requiremodule!App",
        "dojo/text!./templates/TechnologiesSelectionEditor.html"
],
    function (
        dojo,
        array,
        declare,
        Deferred,
        lang,
        DeferredList,
        domAttr,
        domClass,
        domConstruct,
        domStyle,
        query,
        _CssStateMixin,
        _LayoutWidget,
        _TemplatedMixin,
        _Container,
        _Widget,
        _WidgetsInTemplateMixin,
        Tooltip,
        ComboBox,
        FilteringSelect,
        entities,
        _ValueRequiredMixin,
        dependency,
        epi,
        _ContextMixin,
        appModule,
        template
    ) {

        return declare("niteco/editors/TechnologiesSelection", [_Container, _LayoutWidget, _TemplatedMixin, _WidgetsInTemplateMixin, _CssStateMixin, _ValueRequiredMixin, _ContextMixin], {
            templateString: template,

            intermediateChanges: false,

            value: null,

            store: null,

            context: null,
            
            contextId:null,
            
            _tags: [],

            _savevalue: '',

            _autoload: false,

            onChange: function (value) {
                // Event that tells EPiServer when the widget's value has changed.
            },

            postCreate: function () {
                // call base implementation
                this.inherited(arguments);
                if (!this.store) {
                    var registry = dependency.resolve("epi.storeregistry");
                    this.store = this.store || registry.get("technologiesstore");
                }
                this.inputWidget.set("store", this.store);
                this.inputWidget.set("hasDownArrow", false);
                this.inputWidget.set("intermediateChanges", this.intermediateChanges);
                this.connect(this.inputWidget, "onChange", this._onInputWidgetChanged);
            },

            startup: function () {
                if (!this.store) {
                    var registry = dependency.resolve("epi.storeregistry");
                    this.store = this.store || registry.get("technologiesstore");
                }
                this.subscribe("/epi/shell/context/changed", this._contextChanged);
                this.getSaveTags();
            },

            isValid: function () {

                return this.inputWidget.isValid();
            },

            focus: function () {
                // summary:
                //    Put focus on this widget.
                // tags:
                //    public

                this.focus(this.inputWidget);
            },

            // Setter for value property
            _setValueAttr: function (value) {
                this._set('value', value);
                this._tags = [];
            },

            getSaveTags: function () {
                this.context = this.getCurrentContext();
                var contentLink = this.context.id;
                dojo.when(this.store.executeMethod("GetTags", '', { pageID: contentLink }), dojo.hitch(this, function (parent) {
                    array.forEach(parent, lang.hitch(this, "_addDefaultvalue"));
                }));
            },
            _contextChanged: function (newContext) {
                if (newContext) {
                    this.context = newContext;
                    this.contextId = newContext.id;
                }
            },
            _setReadOnlyAttr: function (readOnly) {
                this._set("readOnly", readOnly);
                this.inputWidget.set("readOnly", readOnly);
            },

            _getValueAttr: function () {
                this._savevalue = '';
                if (!this._isEmptyArray(this._tags)) {
                    for (var i = 0; i < this._tags.length; i++) {
                        if (this._tags[i] != null && this._tags[i] != '')
                            this._savevalue += this._tags[i] + ",";
                    }
                }
                return this._savevalue.substring(0, this._savevalue.length - 1);
            },

            // Event handler for the changed event of the input widget
            _onInputWidgetChanged: function (value) {
                this._autoload = true;
                this._updateValue(value);
                this._autoload = false;
            },

            _addButton: function (value) {

                if (value != '') {
                    var name = value;
                    if (query("div[data-epi-tags=" + name + "]", this.TagsGroupContainer).length !== 0) {
                        return;
                    }

                    var containerDiv = domConstruct.create('div', { 'class': 'dijitReset dijitLeft dijitInputField dijitInputContainer epi-categoryButton', id: name });
                    var buttonWrapperDiv = domConstruct.create('div', { 'class': 'dijitInline epi-resourceName' });
                    var categoryNameDiv = domConstruct.create('div', { 'class': 'dojoxEllipsis', innerHTML: entities.encode(value) });
                    domConstruct.place(categoryNameDiv, buttonWrapperDiv);

                    domConstruct.place(buttonWrapperDiv, containerDiv);
                    // create tooltip for the div
                    this._tooltip = new Tooltip({
                        connectId: categoryNameDiv,
                        label: entities.encode(value)
                    });


                    var removeButtonDiv = domConstruct.create('div', { 'class': 'epi-removeButton', innerHTML: '&nbsp;' });
                    domAttr.set(removeButtonDiv, 'data-epi-tags', name);
                    var eventName = removeButtonDiv.onClick ? 'onClick' : 'onclick';

                    if (!this.readOnly) {
                        this.connect(removeButtonDiv, eventName, lang.hitch(this, this._onRemoveClick));
                        domConstruct.place(removeButtonDiv, buttonWrapperDiv);
                    } else {
                        domConstruct.place(domConstruct.create("span", { innerHTML: "&nbsp;" }), buttonWrapperDiv);
                    }

                    domConstruct.place(containerDiv, this.TagsGroupContainer);
                    if (this._tags.indexOf(value) == -1)
                        this._tags.push(value);
                }
            },

            _addDefaultvalue: function (value) {
                this.inputWidget.set("value", value);
                this._addButton(value);
                this.inputWidget.set("value", "");
            },

            _updateValue: function (value) {
                if (this._started && epi.areEqual(this.value, value)) {
                    return;
                }
                //if (this.inputWidget.item != null) {
                //this._set("value", value);
                this._addButton(value);
                this.onChange(value);
                //}
                this.inputWidget.set("value", "");
            },

            _isEmptyArray: function (array) {
                return !array || array.length === 0;
            },

            _onRemoveClick: function (arg) {

                var tag = domAttr.get(arg.target, 'data-epi-tags');
                var tagIndex = this._tags.indexOf(tag);
                if (tagIndex == -1) {
                    return;
                }
                this._tags.splice(tagIndex, 1);
                domConstruct.destroy(tag);
            },
        });
    });