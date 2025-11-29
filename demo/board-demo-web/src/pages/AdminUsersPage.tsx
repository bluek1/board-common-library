import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';

interface User {
  id: number;
  username: string;
  email: string;
  role: string;
  createdAt: string;
  lastLoginAt?: string;
}

export function AdminUsersPage() {
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadUsers();
  }, []);

  const loadUsers = async () => {
    setIsLoading(true);
    try {
      // 백엔드에 사용자 관리 API가 있다면 호출
      // 현재는 더미 데이터 사용
      const dummyUsers: User[] = [
        { id: 1, username: 'admin', email: 'admin@test.com', role: 'Admin', createdAt: '2024-01-01', lastLoginAt: '2024-03-15' },
        { id: 2, username: 'user1', email: 'user1@test.com', role: 'User', createdAt: '2024-01-02', lastLoginAt: '2024-03-14' },
        { id: 3, username: 'user2', email: 'user2@test.com', role: 'User', createdAt: '2024-01-03', lastLoginAt: '2024-03-13' },
        { id: 4, username: 'user3', email: 'user3@test.com', role: 'User', createdAt: '2024-01-04', lastLoginAt: '2024-03-12' },
        { id: 5, username: 'user4', email: 'user4@test.com', role: 'User', createdAt: '2024-01-05', lastLoginAt: '2024-03-11' },
      ];
      setUsers(dummyUsers);
    } catch (err) {
      setError('사용자 목록을 불러오는데 실패했습니다.');
    } finally {
      setIsLoading(false);
    }
  };

  const getRoleBadgeClass = (role: string) => {
    switch (role) {
      case 'Admin':
        return 'bg-red-100 text-red-600';
      case 'Moderator':
        return 'bg-yellow-100 text-yellow-600';
      default:
        return 'bg-blue-100 text-blue-600';
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-gray-800">사용자 관리</h1>
        <Link
          to="/admin"
          className="text-blue-600 hover:underline"
        >
          ← 대시보드로 돌아가기
        </Link>
      </div>

      {error && (
        <div className="p-4 bg-red-50 border border-red-200 text-red-600 rounded-lg">
          {error}
          <button onClick={() => setError('')} className="ml-2 text-red-800">✕</button>
        </div>
      )}

      <div className="bg-white rounded-xl shadow-lg overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50">
              <tr>
                <th className="py-3 px-4 text-left text-sm font-medium text-gray-500">ID</th>
                <th className="py-3 px-4 text-left text-sm font-medium text-gray-500">사용자명</th>
                <th className="py-3 px-4 text-left text-sm font-medium text-gray-500">이메일</th>
                <th className="py-3 px-4 text-left text-sm font-medium text-gray-500">역할</th>
                <th className="py-3 px-4 text-left text-sm font-medium text-gray-500">가입일</th>
                <th className="py-3 px-4 text-left text-sm font-medium text-gray-500">최근 로그인</th>
                <th className="py-3 px-4 text-center text-sm font-medium text-gray-500">관리</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan={7} className="py-8 text-center text-gray-500">
                    로딩 중...
                  </td>
                </tr>
              ) : users.length === 0 ? (
                <tr>
                  <td colSpan={7} className="py-8 text-center text-gray-500">
                    사용자가 없습니다.
                  </td>
                </tr>
              ) : (
                users.map((user) => (
                  <tr key={user.id} className="border-b border-gray-100 hover:bg-gray-50">
                    <td className="py-3 px-4 text-gray-600">{user.id}</td>
                    <td className="py-3 px-4 font-medium text-gray-800">{user.username}</td>
                    <td className="py-3 px-4 text-gray-600">{user.email}</td>
                    <td className="py-3 px-4">
                      <span className={`px-2 py-1 text-xs rounded-full ${getRoleBadgeClass(user.role)}`}>
                        {user.role}
                      </span>
                    </td>
                    <td className="py-3 px-4 text-gray-500 text-sm">
                      {new Date(user.createdAt).toLocaleDateString('ko-KR')}
                    </td>
                    <td className="py-3 px-4 text-gray-500 text-sm">
                      {user.lastLoginAt ? new Date(user.lastLoginAt).toLocaleDateString('ko-KR') : '-'}
                    </td>
                    <td className="py-3 px-4">
                      <div className="flex items-center justify-center gap-2">
                        <button
                          className="px-3 py-1 text-sm text-blue-600 border border-blue-600 rounded hover:bg-blue-50"
                          title="역할 변경"
                        >
                          역할 변경
                        </button>
                        <button
                          className="px-3 py-1 text-sm text-red-600 border border-red-600 rounded hover:bg-red-50"
                          title="비활성화"
                        >
                          비활성화
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* 안내 메시지 */}
      <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
        <div className="flex items-start gap-3">
          <span className="text-yellow-500 text-xl">⚠️</span>
          <div>
            <h3 className="font-medium text-yellow-800">참고</h3>
            <p className="text-sm text-yellow-700 mt-1">
              현재 사용자 관리 API가 구현되어 있지 않아 더미 데이터를 표시하고 있습니다.
              실제 기능은 백엔드 API 구현 후 연동됩니다.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
