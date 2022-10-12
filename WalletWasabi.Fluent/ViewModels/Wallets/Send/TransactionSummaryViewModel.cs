using System.Linq;
using NBitcoin;
using ReactiveUI;
using WalletWasabi.Blockchain.Analysis.Clustering;
using WalletWasabi.Blockchain.TransactionBuilding;
using WalletWasabi.Fluent.Extensions;
using WalletWasabi.Fluent.Helpers;
using WalletWasabi.Wallets;

namespace WalletWasabi.Fluent.ViewModels.Wallets.Send;

public partial class TransactionSummaryViewModel : ViewModelBase
{
	private readonly Wallet _wallet;
	private BuildTransactionResult? _transaction;
	[AutoNotify] private string _amountText = "";
	[AutoNotify] private bool _transactionHasChange;
	[AutoNotify] private bool _transactionHasPockets;
	[AutoNotify] private string _confirmationTimeText = "";
	[AutoNotify] private string _feeText = "";
	[AutoNotify] private bool _maxPrivacy;
	[AutoNotify] private bool _isCustomFeeUsed;
	[AutoNotify] private bool _isOtherPocketSelectionPossible;
	[AutoNotify] private SmartLabel _labels = SmartLabel.Empty;
	[AutoNotify] private SmartLabel _recipient = SmartLabel.Empty;

	public TransactionSummaryViewModel(TransactionPreviewViewModel parent, Wallet wallet, TransactionInfo info, bool isPreview = false)
	{
		Parent = parent;
		_wallet = wallet;
		IsPreview = isPreview;

		this.WhenAnyValue(x => x.TransactionHasChange, x => x.TransactionHasPockets)
			.Subscribe(_ => MaxPrivacy = !TransactionHasPockets && !TransactionHasChange);

		AddressText = info.Destination.ToString();
		PayJoinUrl = info.PayJoinClient?.PaymentUrl.AbsoluteUri;
		IsPayJoin = PayJoinUrl is not null;
	}

	public TransactionPreviewViewModel Parent { get; }

	public bool IsPreview { get; }

	public string AddressText { get; }

	public string? PayJoinUrl { get; }

	public bool IsPayJoin { get; }

	public void UpdateTransaction(BuildTransactionResult transactionResult, TransactionInfo info)
	{
		_transaction = transactionResult;

		ConfirmationTimeText = $"Approximately {TextHelpers.TimeSpanToFriendlyString(info.ConfirmationTimeSpan)} ";

		var destinationAmount = _transaction.CalculateDestinationAmount();
		var btcAmountText = $"{destinationAmount.ToFormattedString()} BTC";
		var exchangeRate = _wallet.Synchronizer.UsdExchangeRate;
		var fiatAmountText = destinationAmount.BtcToUsd(exchangeRate).ToUsdAprox();
		AmountText = $"{btcAmountText} {fiatAmountText}";

		var fee = _transaction.Fee;
		var feeText = fee.ToFeeDisplayUnitString();
		var fiatFeeText = fee.BtcToUsd(exchangeRate).ToUsdAprox();
		FeeText = $"{feeText} {fiatFeeText}";

		TransactionHasChange =
			_transaction.InnerWalletOutputs.Any(x => x.ScriptPubKey != info.Destination.ScriptPubKey);

		Labels = new SmartLabel(transactionResult.SpentCoins.SelectMany(x => x.GetLabels(info.PrivateCoinThreshold)).Except(info.UserLabels.Labels));
		TransactionHasPockets = Labels.Any();

		Recipient = info.UserLabels;

		IsCustomFeeUsed = info.IsCustomFeeUsed;
		IsOtherPocketSelectionPossible = info.IsOtherPocketSelectionPossible;
	}
}
