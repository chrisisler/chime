import { QueryFunctionContext, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import axios, { AxiosError, AxiosResponse } from 'axios';

import { St8 } from './St8';
import { Item } from './interfaces';

const api = {
  items: axios.create({
    baseURL: 'http://localhost:5028' + '/v0/Items',
  }),
};

const queries = {
  items: {
    all: {
      queryKey: ['items'],
      queryFn: ({ signal }: QueryFunctionContext) =>
        api.items.get('/', { signal }).then(r => r.data),
    },
    // findById: (id: number) => ({
    //   queryKey: ['items', id],
    //   queryFn: ({ signal }: QueryFunctionContext) =>
    //     api.items.get(`/${id}`, { signal }).then(r => r.data),
    // })
  },
};

export const useItems = () => St8.from<Item[]>(useQuery(queries.items.all));

// technically providing diff args for diff use cases
// of this function means it ought to be split into to,
// especially from an arity standpoint
export const useCreateItem = () => {
  const queryClient = useQueryClient();

  const m = useMutation({
    /**
     * `type` info communicated via presence of `parentId` field
     */
    mutationFn: ([itemDTO, file]: [Pick<Item, 'by' | 'byId' | 'text' | 'parentId'>, File?]) =>
      api.items.postForm('/', { file, ...itemDTO, }),

    onSuccess: (_: AxiosResponse<Item>) =>
      queryClient.invalidateQueries(queries.items.all),

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
