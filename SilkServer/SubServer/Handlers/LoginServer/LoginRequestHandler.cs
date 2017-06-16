/*
 * АВТОРИЗАЦИЯ
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
	public class LoginRequestHandler : PhotonRequestHandler
	{
		protected ILogger Log = LogManager.GetCurrentClassLogger();

		public LoginRequestHandler(OutboundS2SPeer peer) : base(peer)
		{
		}

		public override void OnHandleRequest(OperationRequest operationRequest)
		{
			var username = (string)operationRequest.Parameters[(byte)UnityParameterCode.Username];
			var password = (string)operationRequest.Parameters[(byte)UnityParameterCode.Password];

			Log.DebugFormat("Login request. Username - {0} | Password - {1}", username, password);

			try
			{
				using (var _session = NHibernateHelper.OpenSession())
				{
					using (var transaction = _session.BeginTransaction())
					{
						var user = _session.CreateCriteria<User>("plu").Add(Restrictions.Eq("plu.Username", username)).UniqueResult<User>();

						Log.DebugFormat("User {0}", user == null ? "NOT FOUND" : user.Username);

						// Авторизируем пользователя
						try
						{
							if (user.Password == BitConverter.ToString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(user.Salt + password))).Replace("-", ""))
							{
								Log.DebugFormat("User is logged in");

								// Ответ после авторизации
								_peer.SendOperationResponse(new OperationResponse(operationRequest.OperationCode, operationRequest.Parameters) { DebugMessage = "Succes", ReturnCode = (short)ErrorCode.Ok }, new SendParameters());
								return;
							}
							else
							{
								// Если пароль не верный
								throw new Exception();
							}
						}
						catch
						{
							_peer.SendOperationResponse(new OperationResponse(operationRequest.OperationCode, operationRequest.Parameters) { ReturnCode = (short)ErrorCode.UsernameOrPasswordInvalid, DebugMessage = "Username or password invalid" }, new SendParameters());
							return;
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Error login user - {0}", e);
			}

			// Если выскочила ошибка при попытке авторизации
			_peer.SendOperationResponse(new OperationResponse(operationRequest.OperationCode, operationRequest.Parameters) { ReturnCode = (short)ErrorCode.UnknownError }, new SendParameters());
		}
	}
}
