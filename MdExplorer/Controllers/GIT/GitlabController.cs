using Ad.Tools.Dal.Extensions;
using Google.Protobuf.Reflection;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Controllers.GIT
{
    [ApiController]
    [Route("/api/gitservices/gitlabsettings")]
    public class GitlabController : ControllerBase
    {
        private readonly IUserSettingsDB _userSettingsDB;

        public GitlabController(IUserSettingsDB userSettingsDB)
        {
            _userSettingsDB = userSettingsDB;
        }

        [HttpGet]
        public IActionResult GetGitlaSettings()
        {
            var settingDal = _userSettingsDB.GetDal<GitlabSetting>();
            var settings = settingDal.GetList().ToArray();
            return Ok(settings);
        }

        [HttpPost]
        public IActionResult PostGitlabSetting([FromBody] GitlabSetting gitlabSetting)
        {
            _userSettingsDB.BeginTransaction();
            var settingDal = _userSettingsDB.GetDal<GitlabSetting>();
            settingDal.Save(gitlabSetting);
            _userSettingsDB.Commit();
            return Ok(new { Message="done!" });
        }

        [HttpPut]
        public IActionResult PutGitlabSetting([FromBody] GitlabSetting gitlabSetting)
        {
            _userSettingsDB.BeginTransaction();
            var settingDal = _userSettingsDB.GetDal<GitlabSetting>();
            settingDal.Save(gitlabSetting);
            _userSettingsDB.Commit();
            return Ok(new { Message = "done!" });
        }

        [HttpGet(":id")]
        public IActionResult GetIdGitlabSetting(Guid id)
        {
            _userSettingsDB.BeginTransaction();
            var settingDal = _userSettingsDB.GetDal<GitlabSetting>();
            var setting = settingDal.GetList().Where(_ => _.Id == id).FirstOrDefault();
            _userSettingsDB.Commit();
            return Ok(setting);
        }
    }
}
