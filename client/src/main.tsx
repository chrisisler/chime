import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { isAxiosError, AxiosError } from 'axios';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';

import App from './App.tsx';
import './index.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
      staleTime: 1000 * 60 * 5,

      // return err from query as state to feed into St8.error,
      // propagating UI reflection of error state
      throwOnError: _err => {
        return false;
      },
    },
    mutations: {
      onError(err: Error) {
        if (!import.meta.env.PROD) {
          if (isAxiosError(err)) {
            console.error(err.response?.data ?? err.message);
          } else {
            console.error(err);
          }
        }
      },
    }
  },
});

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <App />
    </QueryClientProvider>
  </StrictMode>,
);
