using SessionData.SqlServer;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Routing;

namespace SessionData.Mvc.Controllers
{
	public class HomeController : Controller
	{
		private AppSession _appSession = new AppSession(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);

		protected override void Initialize(RequestContext requestContext)
		{
			base.Initialize(requestContext);

			//_appSession.TypeHandlers.Add(typeof(SelectList), ())
		}

		public ActionResult Index()
		{
			var selectList = new SelectList(new SelectListItem[]
			{
				new SelectListItem() { Value = "this", Text = "This" },
				new SelectListItem() { Value = "that", Text = "That"},
				new SelectListItem() { Value = "whatever", Text = "Wahtever" }
			}, "Value", "Text");

			//_appSession["MyDropdown"] = selectList;

			ViewBag.MySelectList = _appSession["MyDropdown"] as SelectList;

			return View();
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}
	}
}