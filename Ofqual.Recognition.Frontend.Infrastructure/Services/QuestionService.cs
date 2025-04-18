﻿using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Enums;
using Ofqual.Recognition.Frontend.Core.Models;
using System.Net.Http.Json;
using Serilog;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services;

public class QuestionService : IQuestionService
{
    private readonly IRecognitionCitizenClient _client;
    private readonly ISessionService _sessionService;

    public QuestionService(IRecognitionCitizenClient recognitionCitizenClient, ISessionService sessionService)
    {
        _client = recognitionCitizenClient;
        _sessionService = sessionService;
    }

    public async Task<QuestionDetails?> GetQuestionDetails(string taskName, string questionName)
    {
        try
        {
            if (_sessionService.HasInSession($"{SessionKeys.ApplicationQuestionDetails}/{taskName}/{questionName}"))
            {
                return _sessionService.GetFromSession<QuestionDetails>($"{SessionKeys.ApplicationQuestionDetails}/{taskName}/{questionName}");
            }

            var client = _client.GetClient();
            var result = await client.GetFromJsonAsync<QuestionDetails>($"/questions/{taskName}/{questionName}");

            if (result == null)
            {
                Log.Warning("No question found with URL: {taskName}/{questionName}", taskName, questionName);
                return result;
            }

            _sessionService.SetInSession($"{SessionKeys.ApplicationQuestionDetails}/{taskName}/{questionName}", result);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while retrieving question for URL: {taskName}/{questionName}", taskName, questionName);
            return null;
        }
    }

    public async Task<QuestionAnswerSubmissionResponse?> SubmitQuestionAnswer(Guid applicationId, Guid taskId, Guid questionId, string answer)
    {
        try
        {
            var client = _client.GetClient();
            var payload = new QuestionAnswerSubmission
            {
                Answer = answer
            };

            var response = await client.PostAsJsonAsync($"/applications/{applicationId}/tasks/{taskId}/questions/{questionId}", payload);

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Failed to submit answer for question {QuestionId} in task {TaskId} of application {ApplicationId}", questionId, taskId, applicationId);
                return null;
            }

            _sessionService.UpdateTaskStatusInSession(taskId, TaskStatusEnum.InProgress);
            
            var result = await response.Content.ReadFromJsonAsync<QuestionAnswerSubmissionResponse>();
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while submitting answer for question {QuestionId} in task {TaskId} of application {ApplicationId}", questionId, taskId, applicationId);
            return null;
        }
    }

    public async Task<List<QuestionAnswerSection>?> GetTaskQuestionAnswers(Guid applicationId, Guid taskId)
    {
        try
        {

            if (_sessionService.HasInSession($"{SessionKeys.ApplicationQuestionReview}/{applicationId}/{taskId}"))
            {
                return _sessionService.GetFromSession<List<QuestionAnswerSection>>($"{SessionKeys.ApplicationQuestionReview}/{applicationId}/{taskId}");
            }

            var client = _client.GetClient();
            var result = await client.GetFromJsonAsync<List<QuestionAnswerSection>>($"/applications/{applicationId}/tasks/{taskId}/questions/answers");

            if (result == null)
            {
                Log.Warning("No question answers found for TaskId: {TaskId} in ApplicationId: {ApplicationId}", taskId, applicationId);
                return null;
            }

            _sessionService.SetInSession($"{SessionKeys.ApplicationQuestionReview}/{applicationId}/{taskId}", result);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while retrieving question answers for TaskId: {TaskId} in ApplicationId: {ApplicationId}", taskId, applicationId);
            return null;
        }
    }
}