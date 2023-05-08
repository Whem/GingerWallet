using System.Threading.Tasks;
using ReactiveUI;
using WalletWasabi.Wallets;

namespace WalletWasabi.Fluent.Models.Wallets;

public partial class WalletAuthModel : ReactiveObject, IWalletAuthModel
{
	private readonly Wallet _wallet;
	[AutoNotify] private bool _isLoggedIn;

	public WalletAuthModel(Wallet wallet)
	{
		_wallet = wallet;
	}

	public bool IsLegalRequired
	{
		get
		{
			var legalRequired = Services.LegalChecker.TryGetNewLegalDocs(out _);
			return legalRequired;
		}
	}

	public async Task<WalletLoginResult> TryLoginAsync(string password)
	{
		string? compatibilityPassword = null;
		var isPasswordCorrect = await Task.Run(() => _wallet.TryLogin(password, out compatibilityPassword));

		var compatibilityPasswordUsed = compatibilityPassword is { };

		return new(isPasswordCorrect, compatibilityPasswordUsed);
	}

	public async Task AcceptTermsAndConditions()
	{
		await Services.LegalChecker.AgreeAsync();
	}

	public void CompleteLogin()
	{
		IsLoggedIn = true;
	}

	public void Logout()
	{
		_wallet.Logout();
	}
}
