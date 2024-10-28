import { QueryFunctionContext, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import axios, { AxiosError, AxiosResponse } from 'axios';

import { St8 } from './St8';
import { Chime, Comment } from './interfaces';

const api = {
  chimes: axios.create({
    baseURL: 'http://localhost:5028' + '/Chimes',
  }),
  comments: axios.create({
    baseURL: 'http://localhost:5028' + '/Comments',
  })
};

const queries = {
  chimes: {
    all: {
      queryKey: ['chimes'],
      queryFn: ({ signal }: QueryFunctionContext) =>
        api.chimes.get('/', { signal }).then(r => r.data),
    },
    // findById: (chimeId: Chime['id']) => ({
    //   queryKey: ['chimes', chimeId],
    //   queryFn: ({ signal }: QueryFunctionContext) =>
    //     api.chimes.get(`/${chimeId}`, { signal }).then(r => r.data),
    // })
  },
  comments: {
    all: {
      queryKey: ['comments'],
      queryFn: ({ signal }: QueryFunctionContext) =>
        api.comments.get('/', { signal }).then(r => r.data),
    }
  }
};

// export const useComments = () => St8.from<Chime[]>(useQuery(queries.chimes.all));
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

export const useCreateComment = () => {
  const queryClient = useQueryClient();

  const m = useMutation({
    mutationFn: ([commentDTO, file]: [Pick<Comment, 'by' | 'byId' | 'text' | 'parentId'>, File?]) =>
      api.comments.postForm('/', { file, ...commentDTO, }),

    onSuccess: (_r: AxiosResponse<Comment>) => 
      queryClient.invalidateQueries(queries.chimes.all),

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
