using Newtonsoft.Json;
using SessionData.SqlServer;
using System;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Routing;

namespace SessionData.Mvc.Controllers
{
	public class HomeController : Controller
	{
		private AppSession _appSession = new AppSession(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
		private UserSession _userSession = null;

		protected override void Initialize(RequestContext requestContext)
		{
			base.Initialize(requestContext);

			_appSession.Serializers.Add(typeof(SelectList), (obj) => SerializeSelectList(obj));
			_appSession.Deserializers.Add(typeof(SelectList), (json) => DeserializeSelectList(json));

			_userSession = new UserSession(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString, Session.SessionID);
		}

		private object DeserializeSelectList(string json)
		{
			throw new NotImplementedException();
		}

		private string SerializeSelectList(object obj)
		{
			SelectList selectList = obj as SelectList;
			if (selectList != null)
			{
				return JsonConvert.SerializeObject(new
				{
					selectList.DataValueField,
					selectList.DataTextField,
					selectList.SelectedValue,
					selectList.SelectedValues,
					selectList.Items
				});
			}

			return null;
		}

		public ActionResult Index()
		{
			var selectList = new SelectList(new SelectListItem[]
			{
				new SelectListItem() { Value = "this", Text = "This" },
				new SelectListItem() { Value = "that", Text = "That"},
				new SelectListItem() { Value = "whatever", Text = "Wahtever" }
			}, "Value", "Text");

			_appSession["MyDropdown"] = selectList;

			ViewBag.MySelectList = selectList;

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