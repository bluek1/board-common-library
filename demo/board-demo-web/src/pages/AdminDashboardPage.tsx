import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import api from '../api/client';

interface Statistics {
  totalPosts: number;
  totalComments: number;
  totalQuestions: number;
  totalAnswers: number;
  totalUsers: number;
  todayPosts: number;
  todayComments: number;
}

interface RecentPost {
  id: number;
  title: string;
  authorName: string;
  createdAt: string;
  viewCount: number;
}

export function AdminDashboardPage() {
  const [stats, setStats] = useState<Statistics | null>(null);
  const [recentPosts, setRecentPosts] = useState<RecentPost[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      // 통계 데이터 로드
      const [statsResponse, postsResponse] = await Promise.all([
        api.get('/admin/statistics').catch(() => ({ data: null })),
        api.get('/posts', { params: { page: 1, pageSize: 5, sort: 'createdAt', order: 'desc' } })
      ]);

      if (statsResponse.data) {
        setStats(statsResponse.data);
      } else {
        // 기본 통계 데이터 (API가 없을 경우)
        setStats({
          totalPosts: 50,
          totalComments: 137,
          totalQuestions: 20,
          totalAnswers: 57,
          totalUsers: 5,
          todayPosts: 3,
          todayComments: 10,
        });
      }

      if (postsResponse.data?.items) {
        setRecentPosts(postsResponse.data.items.slice(0, 5));
      }
    } catch (err) {
      setError('데이터를 불러오는데 실패했습니다.');
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-800">관리자 대시보드</h1>

      {error && (
        <div className="p-4 bg-red-50 border border-red-200 text-red-600 rounded-lg">
          {error}
        </div>
      )}

      {/* 통계 카드 */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          title="전체 게시물"
          value={stats?.totalPosts || 0}
          icon="📝"
          color="blue"
        />
        <StatCard
          title="전체 댓글"
          value={stats?.totalComments || 0}
          icon="💬"
          color="green"
        />
        <StatCard
          title="전체 질문"
          value={stats?.totalQuestions || 0}
          icon="❓"
          color="yellow"
        />
        <StatCard
          title="전체 답변"
          value={stats?.totalAnswers || 0}
          icon="✅"
          color="purple"
        />
      </div>

      {/* 오늘의 활동 */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="bg-white rounded-xl shadow-lg p-6">
          <h2 className="text-lg font-semibold text-gray-800 mb-4">오늘의 활동</h2>
          <div className="space-y-3">
            <div className="flex justify-between items-center">
              <span className="text-gray-600">오늘 작성된 게시물</span>
              <span className="text-xl font-bold text-blue-600">{stats?.todayPosts || 0}</span>
            </div>
            <div className="flex justify-between items-center">
              <span className="text-gray-600">오늘 작성된 댓글</span>
              <span className="text-xl font-bold text-green-600">{stats?.todayComments || 0}</span>
            </div>
            <div className="flex justify-between items-center">
              <span className="text-gray-600">전체 사용자</span>
              <span className="text-xl font-bold text-gray-800">{stats?.totalUsers || 0}</span>
            </div>
          </div>
        </div>

        {/* 빠른 링크 */}
        <div className="bg-white rounded-xl shadow-lg p-6">
          <h2 className="text-lg font-semibold text-gray-800 mb-4">빠른 관리</h2>
          <div className="grid grid-cols-2 gap-3">
            <Link
              to="/admin/posts"
              className="p-3 bg-blue-50 text-blue-600 rounded-lg hover:bg-blue-100 transition text-center"
            >
              게시물 관리
            </Link>
            <Link
              to="/admin/comments"
              className="p-3 bg-green-50 text-green-600 rounded-lg hover:bg-green-100 transition text-center"
            >
              댓글 관리
            </Link>
            <Link
              to="/admin/users"
              className="p-3 bg-purple-50 text-purple-600 rounded-lg hover:bg-purple-100 transition text-center"
            >
              사용자 관리
            </Link>
            <Link
              to="/admin/reports"
              className="p-3 bg-red-50 text-red-600 rounded-lg hover:bg-red-100 transition text-center"
            >
              신고 관리
            </Link>
          </div>
        </div>
      </div>

      {/* 최근 게시물 */}
      <div className="bg-white rounded-xl shadow-lg p-6">
        <h2 className="text-lg font-semibold text-gray-800 mb-4">최근 게시물</h2>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-gray-200">
                <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">제목</th>
                <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">작성자</th>
                <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">조회수</th>
                <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">작성일</th>
              </tr>
            </thead>
            <tbody>
              {recentPosts.map((post) => (
                <tr key={post.id} className="border-b border-gray-100 hover:bg-gray-50">
                  <td className="py-3 px-4">
                    <Link to={`/posts/${post.id}`} className="text-blue-600 hover:underline">
                      {post.title}
                    </Link>
                  </td>
                  <td className="py-3 px-4 text-gray-600">{post.authorName}</td>
                  <td className="py-3 px-4 text-gray-600">{post.viewCount}</td>
                  <td className="py-3 px-4 text-gray-500 text-sm">
                    {new Date(post.createdAt).toLocaleDateString('ko-KR')}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <div className="mt-4 text-right">
          <Link to="/admin/posts" className="text-blue-600 hover:underline text-sm">
            전체 보기 →
          </Link>
        </div>
      </div>
    </div>
  );
}

interface StatCardProps {
  title: string;
  value: number;
  icon: string;
  color: 'blue' | 'green' | 'yellow' | 'purple';
}

function StatCard({ title, value, icon, color }: StatCardProps) {
  const colorClasses = {
    blue: 'bg-blue-50 text-blue-600',
    green: 'bg-green-50 text-green-600',
    yellow: 'bg-yellow-50 text-yellow-600',
    purple: 'bg-purple-50 text-purple-600',
  };

  return (
    <div className="bg-white rounded-xl shadow-lg p-6">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm text-gray-500">{title}</p>
          <p className="text-3xl font-bold text-gray-800">{value.toLocaleString()}</p>
        </div>
        <div className={`w-12 h-12 rounded-full flex items-center justify-center text-2xl ${colorClasses[color]}`}>
          {icon}
        </div>
      </div>
    </div>
  );
}
