﻿using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WalletWasabi.JsonConverters
{
	public class TransactionJsonConverter : JsonConverter
	{
		/// <inheritdoc />
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Transaction);
		}

		/// <inheritdoc />
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var txHex = reader.Value.ToString();
			var tx = Network.Main.Consensus.ConsensusFactory.CreateTransaction();
			tx.FromHex(txHex);
			return tx;
		}

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(((Transaction)value).ToHex());
		}
	}
}
