define(['vendor/handlebars', 'vendor/handlebars-helpers'], function(Handlebars) {

return Handlebars.template({"1":function(depth0,helpers,partials,data) {
    return "        <li></li>\r\n";
},"compiler":[6,">= 2.0.0-beta.1"],"main":function(depth0,helpers,partials,data) {
    var stack1;

  return "<ul class=\"parallax-paging-list\">\r\n"
    + ((stack1 = helpers.each.call(depth0,(depth0 != null ? depth0.list : depth0),{"name":"each","hash":{},"fn":this.program(1, data, 0),"inverse":this.noop,"data":data})) != null ? stack1 : "")
    + "</ul>";
},"useData":true})

});