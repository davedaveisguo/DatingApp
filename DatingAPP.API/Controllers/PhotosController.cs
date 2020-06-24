using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingAPP.API.Data;
using DatingAPP.API.Dtos;
using DatingAPP.API.Helpers;
using DatingAPP.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingAPP.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _clouldinaryConfig;

        private Cloudinary _cloudinary;
        public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> clouldinaryConfig)
        {
            _clouldinaryConfig = clouldinaryConfig;
            _mapper = mapper;
            _repo = repo;

            Account account = new Account(
                _clouldinaryConfig.Value.CloudName,
                 _clouldinaryConfig.Value.ApiKey,
                 _clouldinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }


        [HttpGet("{id}", Name="GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo  =await  _repo.GetPhoto(id);

            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

            return Ok(photo);
        }


        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotoForCreationDto photoForCreationDto)
        {
            // compare userId
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
             return Unauthorized();
            
             var userFromRepo = await _repo.GetUser(userId);

             var file = photoForCreationDto.File;

             var uploadResult = new ImageUploadResult();

             if(file.Length>0)
             {
                 using(var stream = file.OpenReadStream())
                 {
                     var uploadParams = new ImageUploadParams()
                     {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                     };

                     uploadResult = _cloudinary.Upload(uploadParams);
                 }
             }

             photoForCreationDto.Url = uploadResult.Url.ToString();
             photoForCreationDto.PublicId = uploadResult.PublicId;

             var photo  = _mapper.Map<Photo>(photoForCreationDto);

             // set photo to main
             if(!userFromRepo.Photos.Any(u=>u.IsMain))
             {
                 photo.IsMain= true;
             }

             userFromRepo.Photos.Add(photo);

    

             if(await _repo.SaveAll())
             {
                 // put it here cause after save, the id auto generated
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                 return CreatedAtRoute("GetPhoto", new {id = photo.Id}, photoToReturn);
             }

             return BadRequest("Could not add the photo");
             
        }

        // update the main photo
        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            var loginUser = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
             // compare userId
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
             return Unauthorized();

             // if the photo Id not match the photo existed for the user, it not authorized
             var user = await _repo.GetUser(userId);

             if(!user.Photos.Any(p=>p.Id == id))
             {
                 return Unauthorized();
             }

             var photoFromRepo = await _repo.GetPhoto(id);

             if(photoFromRepo.IsMain)
             {
                 return BadRequest("This is already the main photo");
             }

             var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
             // set main photo to false
             currentMainPhoto.IsMain =false; 

             // update new main photo
             photoFromRepo.IsMain = true;

             if(await _repo.SaveAll())
             {
                 return NoContent();
             }

             return BadRequest("Could not set photo to Main");

        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
             // compare userId
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
             return Unauthorized();

             // if the photo Id not match the photo existed for the user, it not authorized
             var user = await _repo.GetUser(userId);

             if(!user.Photos.Any(p=>p.Id == id))
             {
                 return Unauthorized();
             }

             var photoFromRepo = await _repo.GetPhoto(id);

             if(photoFromRepo.IsMain)
             {
                 return BadRequest("You can not delete your main photo");
             }
            
             // if there is publicId  delete from cloundinary
             if(photoFromRepo.PublicId != null)
             {

                var deleteParams = new DeletionParams(photoFromRepo.PublicId);

                var result = _cloudinary.Destroy(deleteParams);

                if(result.Result == "ok")
                {
                    _repo.Delete(photoFromRepo);
                }
             }


             // no publicId just delete from db
             if(photoFromRepo.PublicId == null)
             {
                _repo.Delete(photoFromRepo);
             }


             if(await _repo.SaveAll())
             {
                 return Ok();
             }else
             {
                 return BadRequest("failed to delete the photo");
             }

        }




    }
}