using System.Web.Optimization;

namespace Community.ContenType.DeploymentHubWeb
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/spcontext").Include(
                "~/Scripts/OfficeUiFabric/fabric.js",
                "~/Scripts/spcontext.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/fabric").Include(
                "~/Content/fabric.min.css",
                "~/Content/font-awesome.min.css",
                "~/Content/fabric.components.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/tree").Include(
                "~/Scripts/JsTree/jstree.min.js"));

            bundles.Add(new StyleBundle("~/Content/treestyle").Include(
                "~/Content/JsTree/default/style.css"));
        }
    }
}
