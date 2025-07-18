﻿using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
using System.Net.Http.Json;
using Serilog;
using Ofqual.Recognition.Frontend.Core.Models.ApplicationAnswers;

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

    public async Task<QuestionDetails?> GetQuestionDetails(string taskNameUrl, string questionNameUrl)
    {
        try
        {
            var sessionKey = $"{SessionKeys.ApplicationQuestionDetails}:{taskNameUrl}:{questionNameUrl}";
            if (_sessionService.HasInSession(sessionKey))
            {
                return _sessionService.GetFromSession<QuestionDetails>(sessionKey);
            }

            var client = await _client.GetClientAsync();
            var result = await client.GetFromJsonAsync<QuestionDetails>($"/questions/{taskNameUrl}/{questionNameUrl}");

            if (result == null)
            {
                Log.Warning("No question found with URL: {taskName}/{questionName}", taskNameUrl, questionNameUrl);
                return result;
            }

            _sessionService.SetInSession(sessionKey, result);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while retrieving question for URL: {taskName}/{questionName}", taskNameUrl, questionNameUrl);
            return null;
        }
    }

    public async Task<ValidationResponse?> SubmitQuestionAnswer(Guid applicationId, Guid taskId, Guid questionId, string answer)
    {
        try
        {
            var client = await _client.GetClientAsync();
            var payload = new QuestionAnswerSubmission
            {
                Answer = answer
            };

            var response = await client.PostAsJsonAsync($"/applications/{applicationId}/tasks/{taskId}/questions/{questionId}", payload);
            if (response.IsSuccessStatusCode)
            {
                _sessionService.ClearFromSession($"{SessionKeys.ApplicationQuestionReview}:{applicationId}:{taskId}");
                _sessionService.ClearFromSession($"{SessionKeys.ApplicationQuestionAnswer}:{questionId}:answer");
                _sessionService.ClearFromSession($"{SessionKeys.ApplicationAnswersReview}:{applicationId}");
                _sessionService.UpdateTaskStatusInSession(taskId, StatusType.InProgress);

                return new ValidationResponse();
            }

            var validationResponse = await response.Content.ReadFromJsonAsync<ValidationResponse>();
            if (validationResponse == null)
            {
                Log.Warning("Unable to deserialise the validation response from the application answer submission. QuestionId: {QuestionId}, TaskId: {TaskId}, ApplicationId: {ApplicationId}, StatusCode: {StatusCode}, Reason: {ReasonPhrase}", questionId, taskId, applicationId, response.StatusCode, response.ReasonPhrase);
                return null;
            }

            return validationResponse;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while submitting application answer. QuestionId: {QuestionId}, TaskId: {TaskId}, ApplicationId: {ApplicationId}", questionId, taskId, applicationId);
            return null;
        }
    }

    public async Task<List<QuestionAnswerSection>?> GetTaskQuestionAnswers(Guid applicationId, Guid taskId)
    {
        try
        {
            var sessionKey = $"{SessionKeys.ApplicationQuestionReview}:{applicationId}:{taskId}";
            if (_sessionService.HasInSession(sessionKey))
            {
                return _sessionService.GetFromSession<List<QuestionAnswerSection>>(sessionKey);
            }

            var client = await _client.GetClientAsync();
            var result = await client.GetFromJsonAsync<List<QuestionAnswerSection>>($"/applications/{applicationId}/tasks/{taskId}/questions/answers");

            if (result == null)
            {
                Log.Warning("No question answers found for TaskId: {TaskId} in ApplicationId: {ApplicationId}", taskId, applicationId);
                return null;
            }

            _sessionService.SetInSession(sessionKey, result);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while retrieving question answers for TaskId: {TaskId} in ApplicationId: {ApplicationId}", taskId, applicationId);
            return null;
        }
    }

    public async Task<QuestionAnswer?> GetQuestionAnswer(Guid applicationId, Guid questionId)
    {
        try
        {
            var sessionKey = $"{SessionKeys.ApplicationQuestionAnswer}:{questionId}:answer";
            if (_sessionService.HasInSession(sessionKey))
            {
                return _sessionService.GetFromSession<QuestionAnswer>(sessionKey);
            }

            var client = await _client.GetClientAsync();
            var result = await client.GetFromJsonAsync<QuestionAnswer>($"/applications/{applicationId}/questions/{questionId}/answer");

            if (result == null)
            {
                Log.Warning("No question answer found for questionId: {questionId} in applicationId: {applicationId}", questionId, applicationId);
                return null;
            }

            _sessionService.SetInSession(sessionKey, result);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while retrieving answers for questionId: {questionId} in applicationId: {applicationId}", questionId, applicationId);
            return null;
        }
    }

    public async Task<List<TaskReviewSection>> GetAllApplicationAnswers(Guid applicationId)
    {
        try
        {
            var sessionKey = $"{SessionKeys.ApplicationAnswersReview}:{applicationId}";
            if (_sessionService.HasInSession(sessionKey))
            {
                return _sessionService.GetFromSession<List<TaskReviewSection>>(sessionKey) ?? new List<TaskReviewSection>();
            }

            var client = await _client.GetClientAsync();
            var result = await client.GetFromJsonAsync<List<TaskReviewSection>>($"/applications/{applicationId}/tasks/answers");

            if (result == null)
            {
                Log.Warning("No application answers found for applicationId: {applicationId}",applicationId);
                return new List<TaskReviewSection>();
            }

            _sessionService.SetInSession(sessionKey, result);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while retrieving answers for applicationId: {applicationId}", applicationId);
            return new List<TaskReviewSection>();
        }
    }
}