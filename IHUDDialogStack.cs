public interface IHUDDialogStack
{
	T ShowUIDialog<T>() where T : HUDDialog;
}
