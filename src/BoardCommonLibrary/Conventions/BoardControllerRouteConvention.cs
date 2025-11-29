using BoardCommonLibrary.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace BoardCommonLibrary.Conventions;

/// <summary>
/// 게시판 라이브러리 컨트롤러의 라우트를 동적으로 설정하는 컨벤션
/// </summary>
public class BoardControllerRouteConvention : IControllerModelConvention
{
    private readonly ApiRouteOptions _routeOptions;
    private readonly HashSet<string> _boardControllerNames;
    
    public BoardControllerRouteConvention(ApiRouteOptions routeOptions)
    {
        _routeOptions = routeOptions;
        
        // 게시판 라이브러리에 포함된 컨트롤러 목록
        _boardControllerNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PostsController",
            "CommentsController", 
            "FilesController",
            "SearchController",
            "UsersController",
            "QuestionsController",
            "AnswersController",
            "ReportsController",
            "AdminController"
        };
    }
    
    public void Apply(ControllerModel controller)
    {
        // 게시판 라이브러리 컨트롤러만 처리
        if (!_boardControllerNames.Contains(controller.ControllerType.Name))
        {
            return;
        }
        
        // 컨트롤러 이름에서 "Controller" 제거
        var controllerName = controller.ControllerName;
        
        // 새 라우트 계산
        var newRoute = _routeOptions.GetRoute(controllerName);
        
        // 기존 선택자들의 라우트 업데이트
        foreach (var selector in controller.Selectors)
        {
            if (selector.AttributeRouteModel != null)
            {
                // CommentsController는 특별 처리 (기존 라우트가 "api")
                if (controllerName.Equals("Comments", StringComparison.OrdinalIgnoreCase))
                {
                    // 기본 라우트만 변경, 액션별 라우트는 유지
                    selector.AttributeRouteModel = new AttributeRouteModel
                    {
                        Template = _routeOptions.Prefix
                    };
                }
                else
                {
                    selector.AttributeRouteModel = new AttributeRouteModel
                    {
                        Template = newRoute
                    };
                }
            }
            else
            {
                selector.AttributeRouteModel = new AttributeRouteModel
                {
                    Template = newRoute
                };
            }
        }
        
        // 액션별 라우트도 업데이트 (CommentsController 특별 처리)
        if (controllerName.Equals("Comments", StringComparison.OrdinalIgnoreCase))
        {
            UpdateCommentsControllerActions(controller, _routeOptions);
        }
    }
    
    /// <summary>
    /// CommentsController의 액션별 라우트 업데이트
    /// </summary>
    private void UpdateCommentsControllerActions(ControllerModel controller, ApiRouteOptions options)
    {
        foreach (var action in controller.Actions)
        {
            foreach (var selector in action.Selectors)
            {
                if (selector.AttributeRouteModel?.Template != null)
                {
                    var template = selector.AttributeRouteModel.Template;
                    
                    // posts/{postId}/comments 패턴 업데이트
                    if (template.StartsWith("posts/", StringComparison.OrdinalIgnoreCase))
                    {
                        template = template.Replace("posts/", $"{options.Posts}/", StringComparison.OrdinalIgnoreCase);
                    }
                    
                    // comments/{id} 패턴 업데이트
                    if (template.StartsWith("comments", StringComparison.OrdinalIgnoreCase))
                    {
                        template = template.Replace("comments", options.Comments, StringComparison.OrdinalIgnoreCase);
                    }
                    
                    // questions/{questionId}/comments 패턴 업데이트
                    if (template.StartsWith("questions/", StringComparison.OrdinalIgnoreCase))
                    {
                        template = template.Replace("questions/", $"{options.Questions}/", StringComparison.OrdinalIgnoreCase);
                    }
                    
                    // answers/{answerId}/comments 패턴 업데이트
                    if (template.StartsWith("answers/", StringComparison.OrdinalIgnoreCase))
                    {
                        template = template.Replace("answers/", $"{options.Answers}/", StringComparison.OrdinalIgnoreCase);
                    }
                    
                    selector.AttributeRouteModel.Template = template;
                }
            }
        }
    }
}
