import { QueryFunctionContext, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import axios, { AxiosError, AxiosResponse } from 'axios';

import { St8 } from './St8';
import { Chime } from './interfaces';

const api = {
  chimes: axios.create({
    baseURL: 'http://localhost:5028' + '/Chimes',
  })
};

const queries = {
  chimes: {
    all: {
      queryKey: [null],
      queryFn: ({ signal }: QueryFunctionContext) =>
        api.chimes.get('/', { signal }).then(r => r.data),
    },

    // detail: (id: number) => ({
    //   queryKey: [id],
    //   async queryFn() {
    //     const r = await fetch(API_URL + ApiPaths.Games);
    //     if (!r.ok) throw Error(`${r.status} ${r.statusText}`);
    //     return r.json();
    //   }
    // })
  },
};

export const useChimes = () => St8.from<Chime[]>(useQuery(queries.chimes.all));

export const useCreateChime = () => {
  const queryClient = useQueryClient();

  const m = useMutation({
    mutationFn: ([chimeDTO, file]: [Pick<Chime, 'by' | 'byId' | 'text'>, File?]) =>
      api.chimes.postForm('/', { file, ...chimeDTO, }),

    onSuccess: (r: AxiosResponse<Chime>) =>
      queryClient.setQueryData(queries.chimes.all.queryKey, (_: Chime[] = []) => [r.data].concat(_)),

    onError(err: AxiosError) {
      console.error(err.response?.data ?? err.message);
    },
  });

  return m.mutateAsync;
};

export const formatUnix = (unixTime: number): string => {
  const now = Math.floor(Date.now() / 1000); // Current time in Unix seconds
  const diffInSeconds = now - unixTime;

  if (diffInSeconds < 60) {
    return `${diffInSeconds}s ago`;
  } else if (diffInSeconds < 3600) {
    const minutes = Math.floor(diffInSeconds / 60);
    return `${minutes}m ago`;
  } else if (diffInSeconds < 86400) {
    const hours = Math.floor(diffInSeconds / 3600);
    return `${hours}h ago`;
  } else {
    const days = Math.floor(diffInSeconds / 86400);
    return `${days}d ago`;
  }
};
