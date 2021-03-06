﻿using LiveTv.Vdr.Configuration;
using LiveTv.Vdr.RestfulApi.Resources;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace LiveTv.Vdr.RestfulApi.Client
{
    internal class VdrRestfulApiClient : IRestfulApiClient
    {
        #region Fields

        private string _baseUri;
        private IHttpClient _httpClient;
        private IJsonSerializer _jsonSerializer;

        #endregion

        #region Ctor

        internal VdrRestfulApiClient(IHttpClient httpClient, IJsonSerializer jsonSerializer)
        {
            _baseUri =  Plugin.Instance.Configuration.VDR_RestfulApi_BaseUrl;
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
        }

        #endregion

        #region ResourceRequests

        public async Task<InfoResource> RequestInfoResource(CancellationToken cancellationToken)
        {
            return await RequestResource<InfoResource>(cancellationToken, Constants.ResourceName.Info);
        }

        public async Task<ChannelsResource> RequestChannelsResource(CancellationToken cancellationToken)
        {
            return await RequestResource<ChannelsResource>(cancellationToken, Constants.ResourceName.Channels);
        }

        public async Task<EventsResource> RequestEventsResource(CancellationToken cancellationToken, string channelId, DateTime endDate)
        {
            long span = (long) (endDate.ToLocalTime() - DateTime.Now.ToLocalTime()).TotalSeconds;

            var resourceStr = string.Format("{0}/{1}?timespan={2}", Constants.ResourceName.Events, channelId, span);
            return await RequestResource<EventsResource>(cancellationToken, resourceStr);
        }
        
        public async Task<RecordingsResource> RequestRecordingsResource(CancellationToken cancellationToken)
        {
            return await RequestResource<RecordingsResource>(cancellationToken, Constants.ResourceName.Recordings);
        }

        public async Task<TimersResource> RequestTimersResource(CancellationToken cancellationToken)
        {
            return await RequestResource<TimersResource>(cancellationToken, Constants.ResourceName.Timers);
        }

        #endregion

        private async Task<T> RequestResource<T>(CancellationToken cancellationToken, string resource)  where T  : IRootResource
        {
            var options = new HttpRequestOptions()
            {
                CancellationToken = cancellationToken,
                Url = string.Format("{0}/{1}", _baseUri, resource)
            };

            T restResource;
            using (var stream = await _httpClient.Get(options).ConfigureAwait(false))
            {
                restResource = _jsonSerializer.DeserializeFromStream<T>(stream);
            }

            return restResource;
        }        
    }
}
