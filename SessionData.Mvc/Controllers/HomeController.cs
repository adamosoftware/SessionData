using Newtonsoft.Json;
using SessionData.Mvc.Classes;
using SessionData.SqlServer;
using System;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Routing;

namespace SessionData.Mvc.Controllers
{
	public class HomeController : Controller
	{		
		private SessionDictionary _userSession = null;

		protected override void Initialize(RequestContext requestContext)
		{
			base.Initialize(requestContext);

			_userSession = new SessionDictionary(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString, Session.SessionID);

			_userSession.Serializers.Add(typeof(SelectList), (obj) => JsonConvert.SerializeObject(SelectListSerializer.FromSelectList(obj as SelectList)));
			_userSession.Deserializers.Add(typeof(SelectList), (json) => JsonConvert.DeserializeObject<SelectListSerializer>(json).ToSelectList());
		}

		public ActionResult Index()
		{
			var selectList = new SelectList(new SelectListItem[]
			{
				new SelectListItem() { Value = "this", Text = "This" },
				new SelectListItem() { Value = "that", Text = "That"},
				new SelectListItem() { Value = "whatever", Text = "Whatever" }
			}, "Value", "Text");

			_userSession["MyDropdown"] = selectList;

			ViewBag.MySelectList = selectList;

			return View();
		}

		public ActionResult TestSelectList()
		{
			ViewBag.MySelectList2 = _userSession["MyDropdown"] as SelectList;
			return PartialView();
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