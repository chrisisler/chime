import { formatUnix, useChimes } from './api';
import { CreateChime, Loading } from './components';
import { St8View } from './St8';

export default function App() {
  const chimes = useChimes();

  return (
    <main className="space-y-8 max-w-md mx-auto ">
      <CreateChime />

      <St8View data={chimes} loading={() => <Loading />} error={() => null}>
        {chimes => (
          <>
            {chimes.map(chime => (
              <div
                key={chime.id}
                className="border-gray-600 rounded-md p-8 border space-y-6"
              >
                <div className="flex space-x-4">
                  <div className="h-12 w-12 rounded-full bg-gray-600" />

                  <div className="-mt-1">
                    <p className="font-bold text-lg">{chime.by}</p>
                    <p className="text-gray-600">{formatUnix(chime.time)}</p>
                  </div>
                </div>

                <p className="font-medium">{chime.text}</p>

                <div className="justify-between flex font-medium">
                  <p>15</p>
                  {chime.kids.length
                    ? <p>{chime.kids.length}</p>
                    : null}
                </div>
              </div>
            ))}
          </>
        )}
      </St8View>
    </main >
  )
}
