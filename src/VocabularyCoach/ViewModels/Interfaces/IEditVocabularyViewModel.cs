using System.Windows.Input;

namespace VocabularyCoach.ViewModels.Interfaces
{
	public interface IEditVocabularyViewModel : IPageViewModel
	{
		ICommand GoToStartCommand { get; }
	}
}