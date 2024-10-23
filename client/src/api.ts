import { useQuery } from "@tanstack/react-query";

import { St8 } from "./St8";
import { Chime } from "./interfaces";

const fakeChimes: Chime[] = [
  {
    id: 1,
    deleted: false,
    type: "chime",
    by: "user1",
    byId: 123,
    time: Math.floor(Date.now() / 1000), // Current time in Unix Time
    text: "This is the first chime.",
    parentId: 2,
    mediaUrl: null,
    kids: [],
  },
  {
    id: 2,
    deleted: false,
    type: "chime",
    by: "user2",
    byId: 456,
    time: Math.floor(Date.now() / 1000) - 3600, // One hour ago
    text: "This is the second chime.",
    parentId: 2,
    mediaUrl: null,
    kids: [10, 3, 2],
  },
  {
    id: 3,
    deleted: true,
    type: "chime",
    by: "user3",
    byId: 789,
    time: Math.floor(Date.now() / 1000) - 7200, // Two hours ago
    text: "This chime has been deleted.",
    parentId: 2,
    mediaUrl: null,
    kids: [4, ],
  }
];

const queries = {
  chimes: {
    all: {
      queryKey: [null],
      queryFn: () => fakeChimes,
    },
    // detail: (id: number) => ({
    //   queryKey: [id],
    //   async queryFn() {
    //     const r = await fetch(API_URL + ApiPaths.Games);
    //     if (!r.ok) throw Error(`${r.status} ${r.statusText}`);
    //     return r.json();
    //   }
    // })
  }
};

export const useChimes = () => St8.from<Chime[]>(useQuery(queries.chimes.all));

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
}

