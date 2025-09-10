using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace donacionesWeb.Services.Firebase
{
    public class FirebaseStorageService
    {
        private readonly string _bucketName = "transparenciadonaciones.firebasestorage.app"; // ✅ CORREGIDO
        private readonly string _credentialPath = "wwwroot/keys/transparenciadonaciones-firebase-adminsdk-fbsvc-6279b76baa.json";
        private static readonly object _lock = new();

        public FirebaseStorageService()
        {
            lock (_lock)
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(_credentialPath),
                    });
                }
            }
        }

        public async Task<string> SubirImagenAsync(IFormFile archivo, string carpetaDestino = "imagenes")
        {
            if (archivo == null || archivo.Length == 0)
                throw new ArgumentException("Archivo inválido");

            var nombreArchivo = $"{carpetaDestino}/{Guid.NewGuid()}_{Path.GetFileName(archivo.FileName)}";
            using var stream = archivo.OpenReadStream();

            var credential = GoogleCredential.FromFile(_credentialPath);
            var storage = await StorageClient.CreateAsync(credential);

            // ✅ Tipo MIME correcto para que Firebase y navegadores lo reconozcan como imagen
            await storage.UploadObjectAsync(new Google.Apis.Storage.v1.Data.Object
            {
                Bucket = _bucketName,
                Name = nombreArchivo,
                ContentType = archivo.ContentType
            }, stream);

            return $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{Uri.EscapeDataString(nombreArchivo)}?alt=media";
        }
    }
}
