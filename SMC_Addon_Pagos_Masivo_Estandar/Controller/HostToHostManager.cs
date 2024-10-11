using FluentFTP;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.Controller
{
    public class HostToHostManager
    {
        public static bool SendToSFTP(string host, int port, string user, string pass, string pathFile, string rutaDestino)
        {
            var rspt = false;
            try
            {
                var sftp = new SftpClient(host, port, user, pass);

                sftp.Connect();

                if (sftp.IsConnected)
                {
                    using (var fs = new FileStream(pathFile, FileMode.Open))
                    {
                        sftp.UploadFile(fs, rutaDestino);
                        rspt = true;
                    }
                }
                else
                {
                    throw new InvalidOperationException("No se pudo conectar con el servicio FTP");
                }
                //}
            }
            catch
            {
                throw;
            }
            return rspt;
        }

        public static bool SendToSFTPWithKeySSH(string host, int port, string user, string rutaLlave, string pathFile, string rutaDestino)
        {
            var rspt = false;
            try
            {
                var privateKey = new PrivateKeyFile(rutaLlave);
                var sftp = new SftpClient(host, port, user, privateKey);

                sftp.Connect();

                if (sftp.IsConnected)
                {
                    using (var fs = new FileStream(pathFile, FileMode.Open))
                    {
                        sftp.UploadFile(fs, rutaDestino);
                        rspt = true;
                    }
                }
                else
                {
                    throw new InvalidOperationException("No se pudo conectar con el servicio FTP");
                }
                //}
            }
            catch
            {
                throw;
            }
            return rspt;
        }

        public static bool SendToFTP(string host, int port, string user, string pass, string localPath, string remotePath)
        {
            var rspt = false;
            try
            {
                var client = new FtpClient(host, user, pass, port);
                client.Connect();
                client.UploadFile(localPath, remotePath);
            }
            catch
            {
                throw;
            }
            return rspt;
        }
    }
}
