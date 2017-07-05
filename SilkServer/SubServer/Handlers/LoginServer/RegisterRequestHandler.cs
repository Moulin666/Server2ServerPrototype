/*
 * РЕГИСТРАЦИЯ
 * Created by
 * Roman Khusnetdinov
*/

using System;
using System.Text;
using System.Security.Cryptography;

using ExitGames.Logging;
using Photon.SocketServer;
using Photon.SocketServer.ServerToServer;

using NHibernate.Criterion;

using SilkServerCommon;
using SilkServer.Server2Server;
using SilkServer.SubServer.NHibernate;
using SilkServer.SubServer.NHibernate.Models;

namespace SilkServer.SubServer.Handlers.LoginServer
{
	public class RegisterRequestHandler : PhotonRequestHandler
	{
		protected ILogger Log = LogManager.GetCurrentClassLogger();

		public RegisterRequestHandler(OutboundS2SPeer peer) : base(peer) {}

		public override void OnHandleRequest(OperationRequest operationRequest)
		{
			var username = (string)operationRequest.Parameters[(byte)UnityParameterCode.Username];
			var password = (string)operationRequest.Parameters[(byte)UnityParameterCode.Password];
			var email = (string)operationRequest.Parameters[(byte)UnityParameterCode.Email];
			var characterType = (int)operationRequest.Parameters[(byte)UnityParameterCode.CharacterType];

			try
			{
				using (var _session = NHibernateHelper.OpenSession())
				{
					using (var transaction = _session.BeginTransaction())
					{
						var user = _session.CreateCriteria<User>("plu").Add(Restrictions.Eq("plu.Username", username)).UniqueResult<User>();
						var mail = _session.CreateCriteria<User>("plu").Add(Restrictions.Eq("plu.Email", email)).UniqueResult<User>();

						if (user == null && mail == null)
						{
							var salt = Guid.NewGuid().ToString().Replace("-", "");
							User NewUser = new User()
							{
								Username = username,
								Email = email,
								CharacterType = characterType,
								Money = 5000,
								Password = BitConverter.ToString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(salt + password))).Replace("-", ""),
								Salt = salt,
								Created = DateTime.Now,
								Updated = DateTime.Now
							};

							_session.Save(NewUser);
							transaction.Commit();

							if (Log.IsDebugEnabled)
							{
								Log.DebugFormat("User {0} registered. Password {1}", username, password);
							}

							_peer.SendOperationResponse(new OperationResponse(operationRequest.OperationCode = (byte)UnitySubOperationCode.RegisterSecurely, operationRequest.Parameters)
							{
								ReturnCode = (short)ErrorCode.Ok
							}, new SendParameters());
							return;
						}
						else
						{
							var response = new OperationResponse();

							if (user != null)
							{
								response = new OperationResponse(operationRequest.OperationCode = (byte)UnitySubOperationCode.RegisterSecurely, operationRequest.Parameters)
								{
									ReturnCode = (short)ErrorCode.UsernameInUse,
									DebugMessage = "Username already in use"
								};
							}
							else
							{
								response = new OperationResponse(operationRequest.OperationCode = (byte)UnitySubOperationCode.RegisterSecurely, operationRequest.Parameters)
								{
									ReturnCode = (short)ErrorCode.EmailInUse,
									DebugMessage = "Email already in use"
								};
							}

							_peer.SendOperationResponse(response, new SendParameters());
							transaction.Commit();
							return;
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Error registering user - {0}", e);
			}

			_peer.SendOperationResponse(new OperationResponse(operationRequest.OperationCode = (byte)UnitySubOperationCode.RegisterSecurely, operationRequest.Parameters)
			{
				ReturnCode = (short)ErrorCode.UnknownError
			}, new SendParameters());
		}
	}
}
