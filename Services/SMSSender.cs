using ApplicationSecurityAssignment.Settings;
using Twilio.Types;
using Twilio;
using Microsoft.Extensions.Options;
using Twilio.Rest.Api.V2010.Account;
using Spoonful.Settings;

namespace ApplicationSecurityAssignment.Services
{
	public class SMSSender : ISmsSender
	{
		private readonly SMSoptions _smsConfig;

		public SMSSender(SMSoptions smsConfig)
		{
			_smsConfig = smsConfig;
		}

		public Task SendSmsAsync(string number, string message)
		{
			// Plug in your SMS service here to send a text message.
			// Your Account SID from twilio.com/console
			var accountSid = _smsConfig.SMSAccountIdentification;
			// Your Auth Token from twilio.com/console
			var authToken = _smsConfig.SMSAccountPassword;

			TwilioClient.Init(accountSid, authToken);

			return MessageResource.CreateAsync(
			  to: new PhoneNumber(number),
			  from: new PhoneNumber(_smsConfig.SMSAccountFrom),
			  body: message);
		}
	}
}
