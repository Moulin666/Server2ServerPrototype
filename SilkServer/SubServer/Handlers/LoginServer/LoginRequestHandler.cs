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

		public LoginRequestHandler(OutboundS2SPeer peer) : base(peer) {}

		public override void OnHandleRequest(OperationRequest operationRequest)
		{
			var username = (string)operationRequest.Parameters[(byte)UnityParameterCode.Username];
			var password = (string)operationRequest.Parameters[(byte)UnityParameterCode.Password];

			try
			{
				using (var _session = NHibernateHelper.OpenSession())
				{
					using (var transaction = _session.BeginTransaction())
					{
						var user = _session.CreateCriteria<User>("plu").Add(Restrictions.Eq("plu.Username", username)).UniqueResult<User>();

						try
						{
							if (user.Password == BitConverter.ToString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(user.Salt + password))).Replace("-", ""))
							{
								if (Log.IsDebugEnabled)
								{
									Log.DebugFormat("User {0} is logged in. Password {1}", username, password);
								}

								_peer.SendOperationResponse(new OperationResponse(operationRequest.OperationCode = (byte)UnitySubOperationCode.LoginSecurely, operationRequest.Parameters)
								{
									DebugMessage = "Succes",
									ReturnCode = (short)ErrorCode.Ok
								}, new SendParameters());
								return;
							}
							else
							{
								throw new Exception();
							}
						}
						catch
						{
							_peer.SendOperationResponse(new OperationResponse(operationRequest.OperationCode = (byte)UnitySubOperationCode.LoginSecurely, operationRequest.Parameters)
							{
								DebugMessage = "Username or password invalid",
								ReturnCode = (short)ErrorCode.UsernameOrPasswordInvalid
							}, new SendParameters());
							return;
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Error login user - {0}", e);
			}

			_peer.SendOperationResponse(new OperationResponse(operationRequest.OperationCode = (byte)UnitySubOperationCode.LoginSecurely, operationRequest.Parameters)
			{
				ReturnCode = (short)ErrorCode.UnknownError
			}, new SendParameters());
		}
	}
}
