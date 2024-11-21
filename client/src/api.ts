import { QueryClient, QueryFunctionContext, useQuery, } from '@tanstack/react-query';
import axios, { AxiosResponse, isAxiosError } from 'axios';

import { St8 } from './St8';
import { Item } from './interfaces';

const apiUrl = 'http://' + expect<string>(
  import.meta.env.VITE_AZURE_API_URL,
  'VITE_AZURE_API_URL env var not set, darnit!'
) + ':5001';
// const apiUrl = 'http://localhost:5001';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
      staleTime: 1000 * 60 * 5,

      // return err from query as state to feed into St8.error,
      // propagating UI reflection of error state
      throwOnError: false,
    },
    mutations: {
      onError(err: Error) {
        if (import.meta.env.PROD) {
          return;
        }

        if (isAxiosError(err)) {
          console.error(err.response?.data ?? err.stack ?? err.message);
        } else {
          console.error(err);
        }
      },
    }
  },
});

function expect<T>(cond: T | undefined, msg: string): T {
  if (!cond) {
    throw Error(`Failed Expect: ${msg}`);
  }

  return cond;
}

const api = {
  items: axios.create({
    baseURL: apiUrl + '/v0/Items',
  }),
};

const queries = {
  items: {
    all: {
      queryKey: ['items'],
      queryFn: ({ signal }: QueryFunctionContext) =>
        api.items.get('/', { signal }).then((r: AxiosResponse) => r.data),
    },
    // findById: (id: number) => ({
    //   queryKey: ['items', id],
    //   queryFn: ({ signal }: QueryFunctionContext) =>
    //     api.items.get(`/${id}`, { signal }).then(r => r.data),
    // })
  },
};

// queryClient.prefetchQuery(queries.items.all).catch(err => {
//   console.error(err);
// });

export const useItems = () => St8.from<Item[]>(useQuery(queries.items.all));

export const mutations = {
  createItem: {
    /** `type` info communicated via presence of `parentId` field. */
    mutationFn([itemDTO, file]: [Pick<Item, 'by' | 'byId' | 'text' | 'parentId'>, File?]) {
      return api.items.postForm('/', { file, ...itemDTO, });
    },
    onSuccess() {
      return queryClient.invalidateQueries(queries.items.all);
    },
  },

  deleteItem: {
    mutationFn(item: Item) {
      return api.items.delete(`/${item.id}`);
    },
    onSuccess() {
      return queryClient.invalidateQueries(queries.items.all);
    }
  }
};

export const formatUnix = (unixTime: number, short = false): string => {
  const now = Math.floor(Date.now() / 1000); // Current time in Unix seconds
  const diffInSeconds = now - unixTime;

  const ago = short ? '' : ' ago';

  if (diffInSeconds < 60) {
    return `${diffInSeconds}s${ago}`;
  } else if (diffInSeconds < 3600) {
    const minutes = Math.floor(diffInSeconds / 60);
    return `${minutes}m${ago}`;
  } else if (diffInSeconds < 86400) {
    const hours = Math.floor(diffInSeconds / 3600);
    return `${hours}h${ago}`;
  } else if (diffInSeconds < 2592000) {
    const days = Math.floor(diffInSeconds / 86400);
    return `${days}d${ago}`;
  } else {
    const months = Math.floor(diffInSeconds / 2592000);
    return `${months}mo${ago}`;
  }
};
