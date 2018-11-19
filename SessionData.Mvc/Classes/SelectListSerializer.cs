using System.Linq;
using System.Web.Mvc;

namespace SessionData.Mvc.Classes
{
	public class SelectListSerializer
	{
		public static SelectListSerializer FromSelectList(SelectList selectList)
		{
			return new SelectListSerializer()
			{
				DataValueField = selectList.DataValueField,
				DataTextField = selectList.DataTextField,
				Items = selectList.Items.OfType<SelectListItem>().ToArray(),
				SelectedValue = selectList.SelectedValue
			};
		}

		public static SelectList ToSelectList(SelectListSerializer serializer)
		{
			return new SelectList(serializer.Items, serializer.DataValueField, serializer.DataTextField, serializer.SelectedValue);
		}

		public string DataValueField { get; set; }
		public string DataTextField { get; set; }
		public object SelectedValue { get; set; }
		public SelectListItem[] Items { get; set; }
	}

	public static class SelectListSerializerExtension
	{
		public static SelectList ToSelectList(this SelectListSerializer serializer)
		{
			return SelectListSerializer.ToSelectList(serializer);
		}
	}
}