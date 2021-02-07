using RecipesApi.DTOs;
using RecipesApi.Models;
using System.Collections.Generic;

namespace RecipesApi.Utils
{
    public interface IMediaLogicHelper
    {
        string UserMediasPath { get; }
        int CurrentUserId { get; }
        long MAX_IMAGESIZE { get;  }
        IList<Media> SaveImagesLocally(IEnumerable<MediaDto> medias, out ServiceResponse savingStatus);

        IList<MediaDto> LocateAndLoadMedias(IEnumerable<Media> medias);

        string GenerateUserMediasPath(string mediasDirectoryName);
    }
}