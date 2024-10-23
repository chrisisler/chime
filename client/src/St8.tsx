/**
 * `St8` provides a structured way to model and manage data states, reducing the likelihood of
 * errors and improving the standardizaiton and maintainability of the code.
 */
const St8Empty = "St8::Empty" as const;
const St8Loading = "St8::Loading" as const;

export type St8<T> = typeof St8Empty | typeof St8Loading | Error | T;

/** Wait for all provided St8s to be ready. */
// @ts-expect-error Ignoring arg type def.
const _all: St8All = (...array) =>
  array.find(arg => !St8.isReady(arg)) || array;

interface St8All {
  <A>(ds1: St8<A>): St8<[A]>;
  <A, B>(ds1: St8<A>, ds2: St8<B>): St8<[A, B]>;
  <A, B, C>(ds1: St8<A>, ds2: St8<B>, ds3: St8<C>): St8<[A, B, C]>;
  <A, B, C, D>(
    ds1: St8<A>,
    ds2: St8<B>,
    ds3: St8<C>,
    ds4: St8<D>,
  ): St8<[A, B, C, D]>;
}

// eslint-disable-next-line
export const St8 = {
  Empty: St8Empty,
  Loading: St8Loading,
  error: (err: unknown): Error => {
    return err instanceof Error ? err : Error(String(err));
  },
  isEmpty<T>(arg: St8<T>): arg is typeof St8Empty {
    return arg === St8Empty;
  },
  isLoading<T>(arg: St8<T>): arg is typeof St8Loading {
    return arg === St8Loading;
  },
  isError<T>(arg: St8<T>): arg is Error {
    return arg instanceof Error;
  },
  isReady<T>(arg: St8<T>): arg is T {
    return !St8.isError(arg) && !St8.isLoading(arg) && arg !== St8Empty;
  },
  map<T, U>(arg: St8<T>, fn: (arg: T) => St8<U>): St8<U> {
    if (!St8.isReady(arg)) return arg as St8<U>;
    return fn(arg);
  },
  from<T>(state: {
    isLoading: boolean;
    error: unknown;
    data: T | undefined;
  }): St8<T> {
    if (state.error) return St8.error(state.error);
    if (state.isLoading) return St8.Loading;
    if (state.data === undefined) return St8.Empty;
    return state.data;
  },
  /**
   * Converts an array of St8 into a St8<T[]>.
   *
   * Providing generics is required for TypeScript to recognize the ready
   * result.
   *
   * @example St8.all<[string, number]>(str, num);
   */
  all: _all,
};

export function St8View<T>(props: {
  data: St8<T>;
  children: (data: T) => JSX.Element | null;
  loading: () => JSX.Element | null;
  error: () => JSX.Element | null;
  empty?: () => JSX.Element | null;
}): JSX.Element | null {
  if (St8.isEmpty(props.data)) return props.empty?.() ?? null;
  if (St8.isLoading(props.data)) return props.loading();
  if (St8.isError(props.data)) return props.error();
  return props.children(props.data);
}
