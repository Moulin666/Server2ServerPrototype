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

using SilkServerCommon;
using SilkServer.Server2Server;
using SilkServer.NHibernate;
using SilkServer.NHibernate.Models;
using System.Collections.Generic;

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
						var user = _session.QueryOver<Account>().Where(n => n.Username == username).SingleOrDefault();

						try
						{
							if (user.Password == BitConverter.ToString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(user.Salt + password))).Replace("-", ""))
							{
								if (Log.IsDebugEnabled)
								{
									Log.DebugFormat("User {0} is logged in. Password {1}", username, password);
								}

								var character = _session.QueryOver<Character>().Where(n => n.Account == user).SingleOrDefault();

								if (character != null)
								{
									var parameters = new Dictionary<byte, object>
									{
										{ (byte)ParameterCode.UserId, operationRequest.Parameters[(byte)ParameterCode.UserId] },
										{ (byte)UnityParameterCode.CharacterType, character.CharacterType },
										{ (byte)UnityParameterCode.Money, character.Money },
										{ (byte)UnityParameterCode.Exp, character.Exp },
										{ (byte)UnityParameterCode.Wins, character.Wins },
										{ (byte)UnityParameterCode.Defeats, character.Defeats },
										{ (byte)UnityParameterCode.Kills, character.Kills },
										{ (byte)UnityParameterCode.Deaths, character.Deaths }
									};

									var response = new OperationResponse(operationRequest.OperationCode = (byte)UnitySubOperationCode.LoginSecurely, parameters)
									{
										ReturnCode = (byte)UnityErrorCode.ok
									};

									_peer.SendOperationResponse(response, new SendParameters());
									return;
								}
								else
								{
									Log.ErrorFormat("Can't get character from database. username - {0}", user.Username);
								}
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
								ReturnCode = (short)UnityErrorCode.UsernameOrPasswordInvalid
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
