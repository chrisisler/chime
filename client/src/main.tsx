import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { isAxiosError } from 'axios';
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

      throwOnError: err => {
        if (!import.meta.env.PROD) {
          if (isAxiosError(err)) {
            console.error(err.response?.data ?? err);
          } else {
            console.error(err);
          }
        }

        // return err from query as state to feed into St8.error,
        // propagating UI reflection of error state
        return false;
      },
    },
    // TODO
    // mutations: {
    //   onError:
    // }
  },
});

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <App />
    </QueryClientProvider>
  </StrictMode>,
);
